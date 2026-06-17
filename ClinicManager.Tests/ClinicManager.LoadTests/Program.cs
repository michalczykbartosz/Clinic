using System.Diagnostics;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

const int ConcurrentUsers = 50;
const int TotalRequests = 100;
const string EndpointPath = "/api/visits/active";

QuestPDF.Settings.License = LicenseType.Community;

var baseUrl = GetArgumentValue(args, "--base-url")
    ?? Environment.GetEnvironmentVariable("CLINIC_BASE_URL")
    ?? "http://localhost:5174";

var reportFolder = GetArgumentValue(args, "--report-folder")
    ?? Environment.GetEnvironmentVariable("CLINIC_LOAD_REPORT_FOLDER")
    ?? Path.Combine(AppContext.BaseDirectory, "reports");

var endpointUrl = new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), EndpointPath.TrimStart('/'));
Directory.CreateDirectory(reportFolder);

var reportFileName = $"tech7-active-visits-{DateTime.Now:yyyyMMdd-HHmmss}";
var pdfReportPath = Path.Combine(reportFolder, $"{reportFileName}.pdf");

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromSeconds(30)
};

var metrics = new LoadTestMetrics(endpointUrl.ToString(), ConcurrentUsers, TotalRequests);

var scenario = Scenario.Create("active_visits_api", async context =>
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        using var response = await httpClient.GetAsync(EndpointPath);
        stopwatch.Stop();

        var responseSize = response.Content.Headers.ContentLength ?? 0;
        metrics.Record(response.IsSuccessStatusCode, stopwatch.Elapsed, (int)response.StatusCode);

        return response.IsSuccessStatusCode
            ? Response.Ok(statusCode: ((int)response.StatusCode).ToString(), sizeBytes: responseSize)
            : Response.Fail(statusCode: ((int)response.StatusCode).ToString(), sizeBytes: responseSize);
    }
    catch (Exception exception)
    {
        stopwatch.Stop();
        metrics.Record(success: false, stopwatch.Elapsed, statusCode: null);

        return Response.Fail(message: exception.Message);
    }
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.IterationsForConstant(copies: ConcurrentUsers, iterations: TotalRequests));

NBomberRunner
    .RegisterScenarios(scenario)
    .WithTestSuite("ClinicManager")
    .WithTestName("TECH7 aktywne wizyty API")
    .WithReportFolder(reportFolder)
    .WithReportFileName(reportFileName)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
    .Run();

LoadTestPdfReport.Generate(pdfReportPath, metrics.Snapshot());

Console.WriteLine($"Raport NBomber: {reportFolder}");
Console.WriteLine($"Raport PDF: {pdfReportPath}");

static string? GetArgumentValue(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

internal sealed class LoadTestMetrics
{
    private readonly object _lock = new();
    private readonly Dictionary<int, int> _statusCodes = [];
    private readonly string _endpointUrl;
    private readonly int _concurrentUsers;
    private readonly int _configuredRequests;
    private readonly Stopwatch _testDuration = Stopwatch.StartNew();

    private int _completedRequests;
    private int _successfulRequests;
    private int _failedRequests;
    private long _totalLatencyTicks;
    private TimeSpan _minLatency = TimeSpan.MaxValue;
    private TimeSpan _maxLatency = TimeSpan.Zero;

    public LoadTestMetrics(string endpointUrl, int concurrentUsers, int configuredRequests)
    {
        _endpointUrl = endpointUrl;
        _concurrentUsers = concurrentUsers;
        _configuredRequests = configuredRequests;
    }

    public void Record(bool success, TimeSpan latency, int? statusCode)
    {
        lock (_lock)
        {
            _completedRequests++;
            _totalLatencyTicks += latency.Ticks;

            if (success)
            {
                _successfulRequests++;
            }
            else
            {
                _failedRequests++;
            }

            if (latency < _minLatency)
            {
                _minLatency = latency;
            }

            if (latency > _maxLatency)
            {
                _maxLatency = latency;
            }

            if (statusCode.HasValue)
            {
                _statusCodes.TryGetValue(statusCode.Value, out var count);
                _statusCodes[statusCode.Value] = count + 1;
            }
        }
    }

    public LoadTestSummary Snapshot()
    {
        lock (_lock)
        {
            _testDuration.Stop();

            var averageLatency = _completedRequests == 0
                ? TimeSpan.Zero
                : TimeSpan.FromTicks(_totalLatencyTicks / _completedRequests);

            return new LoadTestSummary(
                EndpointUrl: _endpointUrl,
                ConcurrentUsers: _concurrentUsers,
                ConfiguredRequests: _configuredRequests,
                CompletedRequests: _completedRequests,
                SuccessfulRequests: _successfulRequests,
                FailedRequests: _failedRequests,
                MinLatency: _completedRequests == 0 ? TimeSpan.Zero : _minLatency,
                AverageLatency: averageLatency,
                MaxLatency: _maxLatency,
                Duration: _testDuration.Elapsed,
                GeneratedAt: DateTime.Now,
                StatusCodes: _statusCodes.OrderBy(pair => pair.Key).ToDictionary());
        }
    }
}

internal sealed record LoadTestSummary(
    string EndpointUrl,
    int ConcurrentUsers,
    int ConfiguredRequests,
    int CompletedRequests,
    int SuccessfulRequests,
    int FailedRequests,
    TimeSpan MinLatency,
    TimeSpan AverageLatency,
    TimeSpan MaxLatency,
    TimeSpan Duration,
    DateTime GeneratedAt,
    IReadOnlyDictionary<int, int> StatusCodes);

internal static class LoadTestPdfReport
{
    public static void Generate(string filePath, LoadTestSummary summary)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(text => text.FontSize(10));

                page.Header()
                    .Text("TECH7 - raport testu obciążeniowego")
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text($"Endpoint: {summary.EndpointUrl}");
                    column.Item().Text($"Wygenerowano: {summary.GeneratedAt:yyyy-MM-dd HH:mm:ss}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        AddRow(table, "Równolegli użytkownicy", summary.ConcurrentUsers.ToString());
                        AddRow(table, "Skonfigurowane żądania", summary.ConfiguredRequests.ToString());
                        AddRow(table, "Wykonane żądania", summary.CompletedRequests.ToString());
                        AddRow(table, "Poprawne odpowiedzi", summary.SuccessfulRequests.ToString());
                        AddRow(table, "Błędne odpowiedzi", summary.FailedRequests.ToString());
                        AddRow(table, "Czas testu", FormatDuration(summary.Duration));
                        AddRow(table, "Min. czas odpowiedzi", FormatDuration(summary.MinLatency));
                        AddRow(table, "Śr. czas odpowiedzi", FormatDuration(summary.AverageLatency));
                        AddRow(table, "Maks. czas odpowiedzi", FormatDuration(summary.MaxLatency));
                    });

                    column.Item().Text("Kody odpowiedzi").FontSize(13).Bold();

                    if (summary.StatusCodes.Count == 0)
                    {
                        column.Item().Text("Brak zarejestrowanych kodów odpowiedzi.");
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            AddRow(table, "Kod HTTP", "Liczba odpowiedzi", header: true);

                            foreach (var (statusCode, count) in summary.StatusCodes)
                            {
                                AddRow(table, statusCode.ToString(), count.ToString());
                            }
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text("ClinicManager LoadTests / NBomber")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf(filePath);
    }

    private static void AddRow(TableDescriptor table, string label, string value, bool header = false)
    {
        var background = header ? Colors.Blue.Lighten4 : Colors.White;
        var labelText = table.Cell()
            .Background(background)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(6)
            .Text(label);

        var valueText = table.Cell()
            .Background(background)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(6)
            .Text(value);

        if (header)
        {
            labelText.Bold();
            valueText.Bold();
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalMilliseconds < 1000
            ? $"{duration.TotalMilliseconds:0.##} ms"
            : $"{duration.TotalSeconds:0.##} s";
    }
}
