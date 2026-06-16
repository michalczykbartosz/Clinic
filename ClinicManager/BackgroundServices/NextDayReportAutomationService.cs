using MailKit;
using ClinicManager.Data;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace ClinicManager.BackgroundServices;



public class NextDayReportAutomationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NextDayReportAutomationService> _logger;

    public NextDayReportAutomationService(
        IServiceScopeFactory scopeFactory,
        ILogger<NextDayReportAutomationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        _logger.LogInformation("Uruchomiono usługę generowania raportu wizyt na kolejny dzień.");

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
                    var tomorrow = DateTime.Today.AddDays(1);
                    DateTime startTomorrow = tomorrow;
                    DateTime endTomorrow = tomorrow.AddDays(1).AddTicks(-1);

                    _logger.LogInformation("Rozpoczęto generowanie raportu wizyt na dzień {ReportDate}", tomorrow);
                    
                    var tomorrowVisitsCount = await context.Visits
                        .Where(v => v.VisitDateTime >= startTomorrow && v.VisitDateTime <= endTomorrow)
                        .CountAsync(stoppingToken);
                    ;
                    var tomorrowVisits = await context.Visits
                        .Include(v => v.Doctor)
                        .Include(v => v.Patient)
                        .Where(v => v.VisitDateTime >= startTomorrow && v.VisitDateTime <= endTomorrow)
                        .OrderBy(v => v.VisitDateTime)
                        .ToListAsync(stoppingToken);
                    if (tomorrowVisitsCount > 0)
                    {
                        var pdfBytes = Document.Create(container =>
                        {
                            container.Page(page =>
                            {
                                page.Size(PageSizes.A4);
                                page.Margin(2, Unit.Centimetre);
                                page.PageColor(Colors.White);
                                page.DefaultTextStyle(x => x.FontSize(11));

                                page.Header().Text($"Raport wizyt na dzień {tomorrow:dd.MM.yyyy}").SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                                {
                                    column.Spacing(5);
            
                                    foreach (var visit in tomorrowVisits)
                                    {
                                        column.Item().Row(row =>
                                        {
                                            row.ConstantItem(50).Text($"{visit.VisitDateTime:HH:mm}");
                                            row.RelativeItem().Text($"Pacjent: {visit.Patient.FirstName} {visit.Patient.LastName}");
                                            row.RelativeItem().Text($"Lekarz: {visit.Doctor.FirstName} {visit.Doctor.LastName}");
                                        });
                                    }
                                });

                                page.Footer().AlignCenter().Text(x =>
                                {
                                    x.CurrentPageNumber();
                                    x.Span(" / ");
                                    x.TotalPages();
                                });
                            });
                        }).GeneratePdf(); 
                        
                        var message = new MimeMessage();
    message.From.Add(new MailboxAddress("System ClinicManager", "clinic@example.com"));
    message.To.Add(new MailboxAddress("Administrator", "admin@wp.pl")); // adres docelowy
    message.Subject = $"Automatyczny raport wizyt - {tomorrow:dd.MM.yyyy}";

    var body = new TextPart("plain")
    {
        Text = $"Dzień dobry,\n\nw załączniku znajduje się automatycznie wygenerowany raport wizyt na dzień {tomorrow:dd.MM.yyyy}.\n\nLiczba zaplanowanych wizyt: {tomorrowVisitsCount}.\n\nPozdrawiamy,\nSystem ClinicManager"
    };

    var attachment = new MimePart("application", "pdf")
    {
        Content = new MimeContent(new MemoryStream(pdfBytes)),
        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
        ContentTransferEncoding = ContentEncoding.Base64,
        FileName = $"Raport_Wizyt_{tomorrow:yyyyMMdd}.pdf"
    };
    
    var multipart = new MimeKit.Multipart("mixed");
    multipart.Add(body);
    multipart.Add(attachment);
    message.Body = multipart;

    using (var client = new MailKit.Net.Smtp.SmtpClient())
    {
        await client.ConnectAsync("localhost", 1025, MailKit.Security.SecureSocketOptions.None, stoppingToken);        
        await client.SendAsync(message, stoppingToken);
        await client.DisconnectAsync(true, stoppingToken);
    }
                        _logger.LogInformation(
                            "Wysłano automatyczny raport wizyt na dzień {ReportDate}. Liczba wizyt: {VisitCount}",
                            tomorrow,
                            tomorrowVisitsCount);
                    }
                    else
                    {
                        _logger.LogInformation("Brak wizyt do raportu na dzień {ReportDate}", tomorrow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nie udało się wygenerować lub wysłać automatycznego raportu wizyt.");
            }
        }
    }
}
