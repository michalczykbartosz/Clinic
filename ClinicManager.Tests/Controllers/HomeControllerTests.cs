using ClinicManager.Controllers;
using ClinicManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicManager.Tests;

public class HomeControllerTests
{
    [Test]
    public void Index_ReturnsView()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance);

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Privacy_ReturnsView()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance);

        var result = controller.Privacy();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Error_ReturnsRequestIdInModel()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = "request-1" }
            }
        };

        var result = controller.Error() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That((result!.Model as ErrorViewModel)!.RequestId, Is.EqualTo("request-1"));
    }
}
