using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.Extensions.Logging;
using DocHub.Core.Entities;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneratedLettersController : ControllerBase
{
    private readonly IGeneratedLetterService _letterService;
    private readonly ILogger<GeneratedLettersController> _logger;

    public GeneratedLettersController(
        IGeneratedLetterService letterService,
        ILogger<GeneratedLettersController> logger)
    {
        _letterService = letterService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var letters = await _letterService.GetPagedAsync(page, pageSize);
            var totalCount = await _letterService.GetTotalCountAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-PageSize", pageSize.ToString());

            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all generated letters");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search term is required");
            }

            var letters = await _letterService.SearchAsync(q);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching letters with term: {SearchTerm}", q);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetByStatus(string status)
    {
        try
        {
            var letters = await _letterService.GetByStatusAsync(status);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by status: {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetByEmployee(string employeeId)
    {
        try
        {
            var letters = await _letterService.GetByEmployeeAsync(employeeId);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by employee: {EmployeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("template/{templateId}")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetByTemplate(string templateId)
    {
        try
        {
            var letters = await _letterService.GetByTemplateAsync(templateId);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by template: {TemplateId}", templateId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var letters = await _letterService.GetByDateRangeAsync(startDate, endDate);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by date range: {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneratedLetter>> GetById(string id)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound();
            }
            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("letter-number/{letterNumber}")]
    public async Task<ActionResult<GeneratedLetter>> GetByLetterNumber(string letterNumber)
    {
        try
        {
            var letter = await _letterService.GetByLetterNumberAsync(letterNumber);
            if (letter == null)
            {
                return NotFound();
            }
            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter by number: {LetterNumber}", letterNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<GeneratedLetter>> Create([FromBody] GeneratedLetter letter)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdLetter = await _letterService.CreateAsync(letter);
            return CreatedAtAction(nameof(GetById), new { id = createdLetter.Id }, createdLetter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating generated letter");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedLetter>> GenerateLetter(
        [FromBody] GenerateLetterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var generatedLetter = await _letterService.GenerateLetterAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = generatedLetter.Id }, generatedLetter);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/send-email")]
    public async Task<ActionResult> SendEmail(
        string id, 
        [FromBody] SendEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _letterService.SendEmailAsync(id, request.EmailId, request.AttachmentPaths);
            if (result)
            {
                return Ok(new { message = "Email sent successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to send email" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/resend-email")]
    public async Task<ActionResult> ResendEmail(string id)
    {
        try
        {
            var result = await _letterService.ResendEmailAsync(id);
            if (result)
            {
                return Ok(new { message = "Email resent successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to resend email" });
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email for letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GeneratedLetter>> Update(string id, [FromBody] GeneratedLetter letter)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedLetter = await _letterService.UpdateAsync(id, letter);
            return Ok(updatedLetter);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var result = await _letterService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/update-status")]
    public async Task<ActionResult> UpdateStatus(
        string id, 
        [FromBody] UpdateStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _letterService.UpdateStatusAsync(id, request.Status, request.ErrorMessage);
            if (result)
            {
                return Ok(new { message = "Status updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update status" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}


