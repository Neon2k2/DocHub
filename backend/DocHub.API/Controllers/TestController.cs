using Microsoft.AspNetCore.Mvc;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("DocHub Backend is working! 🎉");
    }

    [HttpGet("health")]
    public ActionResult<object> HealthCheck()
    {
        var healthStatus = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Database = "Available",
            Services = new
            {
                Core = "Available",
                API = "Available"
            }
        };

        return Ok(healthStatus);
    }

    [HttpGet("config")]
    public ActionResult<object> GetConfiguration()
    {
        var config = new
        {
            Database = new
            {
                UseSqlServer = false,
                ConnectionString = "Data Source=DocHub.db;Mode=ReadWriteCreate;"
            },
            Features = new
            {
                Email = "SendGrid",
                Document = "Syncfusion",
                Signature = "PROXKey"
            }
        };

        return Ok(config);
    }
}
