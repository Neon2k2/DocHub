using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IEmailService emailService, ILogger<WebhookController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Handle SendGrid webhook events for email tracking
    /// </summary>
    [HttpPost("sendgrid")]
    public async Task<IActionResult> SendGridWebhook()
    {
        try
        {
            // Read the webhook payload
            using var reader = new StreamReader(Request.Body);
            var webhookPayload = await reader.ReadToEndAsync();

            _logger.LogInformation("Received SendGrid webhook: {PayloadLength} characters", webhookPayload.Length);

            // Process the webhook event
            var success = await _emailService.ProcessWebhookEventAsync(webhookPayload);

            if (success)
            {
                _logger.LogInformation("SendGrid webhook processed successfully");
                return Ok(new { status = "success", message = "Webhook processed successfully" });
            }
            else
            {
                _logger.LogWarning("SendGrid webhook processing failed");
                return BadRequest(new { status = "error", message = "Webhook processing failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid webhook");
            return StatusCode(500, new { status = "error", message = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle generic webhook events
    /// </summary>
    [HttpPost("generic")]
    public async Task<IActionResult> GenericWebhook([FromBody] object webhookData)
    {
        try
        {
            _logger.LogInformation("Received generic webhook: {WebhookData}", webhookData);

            // Process generic webhook data
            // This can be extended to handle other webhook types

            return Ok(new { status = "success", message = "Generic webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing generic webhook");
            return StatusCode(500, new { status = "error", message = "Internal server error" });
        }
    }

    /// <summary>
    /// Verify webhook endpoint (for testing)
    /// </summary>
    [HttpGet("verify")]
    public IActionResult VerifyWebhook()
    {
        return Ok(new { 
            status = "success", 
            message = "Webhook endpoint is active",
            timestamp = DateTime.UtcNow,
            service = "DocHub Webhook Service"
        });
    }
}
