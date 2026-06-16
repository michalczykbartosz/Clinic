using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.Models;

namespace ClinicManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Wyświetlono stronę błędu dla żądania {RequestId}", requestId);

        return View(new ErrorViewModel { RequestId = requestId });
    }
}
