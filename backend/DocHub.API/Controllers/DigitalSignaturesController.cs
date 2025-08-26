using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DigitalSignaturesController : ControllerBase
{
    private readonly IDigitalSignatureService _signatureService;
    private readonly ILogger<DigitalSignaturesController> _logger;

    public DigitalSignaturesController(
        IDigitalSignatureService signatureService,
        ILogger<DigitalSignaturesController> logger)
    {
        _signatureService = signatureService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DigitalSignature>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var signatures = await _signatureService.GetPagedAsync(page, pageSize);
            var totalCount = await _signatureService.GetTotalCountAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-PageSize", pageSize.ToString());

            return Ok(signatures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all digital signatures");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DigitalSignature>>> GetActive()
    {
        try
        {
            var signatures = await _signatureService.GetActiveSignaturesAsync();
            return Ok(signatures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active signatures");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("latest")]
    public async Task<ActionResult<string>> GetLatestSignaturePath()
    {
        try
        {
            var signaturePath = await _signatureService.GetLatestSignaturePathAsync();
            if (string.IsNullOrEmpty(signaturePath))
            {
                return NotFound("No active signatures found");
            }
            return Ok(new { signaturePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest signature path");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DigitalSignature>> GetById(string id)
    {
        try
        {
            var signature = await _signatureService.GetByIdAsync(id);
            if (signature == null)
            {
                return NotFound();
            }
            return Ok(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("authority/{authorityName}")]
    public async Task<ActionResult<DigitalSignature>> GetByAuthorityName(string authorityName)
    {
        try
        {
            var signature = await _signatureService.GetByAuthorityNameAsync(authorityName);
            if (signature == null)
            {
                return NotFound();
            }
            return Ok(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature by authority name: {AuthorityName}", authorityName);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetSignatureImage(string id)
    {
        try
        {
            var imageData = await _signatureService.GetSignatureImageAsync(id);
            return File(imageData, "image/png");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Signature image file not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature image for id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DigitalSignature>> Create([FromBody] DigitalSignature signature)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdSignature = await _signatureService.CreateAsync(signature);
            return CreatedAtAction(nameof(GetById), new { id = createdSignature.Id }, createdSignature);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating digital signature");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate-from-proxkey")]
    public async Task<ActionResult<DigitalSignature>> GenerateFromPROXKey(
        [FromBody] GenerateSignatureRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var generatedSignature = await _signatureService.GenerateSignatureFromPROXKeyAsync(
                request.AuthorityName, 
                request.AuthorityDesignation);

            return CreatedAtAction(nameof(GetById), new { id = generatedSignature.Id }, generatedSignature);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature from PROXKey");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DigitalSignature>> Update(string id, [FromBody] UpdateDigitalSignatureDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert DTO to entity
            var signature = new DigitalSignature
            {
                Id = id,
                SignatureName = updateDto.SignatureName,
                AuthorityName = updateDto.AuthorityName,
                AuthorityDesignation = updateDto.AuthorityDesignation,
                SignatureImagePath = updateDto.SignatureImagePath,
                SignatureData = updateDto.SignatureData,
                SignatureDate = updateDto.SignatureDate,
                IsActive = updateDto.IsActive,
                SortOrder = updateDto.SortOrder
            };

            var updatedSignature = await _signatureService.UpdateAsync(id, signature);
            return Ok(updatedSignature);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating signature: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var result = await _signatureService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting signature: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(string id)
    {
        try
        {
            var result = await _signatureService.ToggleActiveAsync(id);
            if (!result)
            {
                return NotFound();
            }

            var signature = await _signatureService.GetByIdAsync(id);
            return Ok(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling signature status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/reorder")]
    public async Task<ActionResult> Reorder(string id, [FromBody] int newSortOrder)
    {
        try
        {
            var signature = await _signatureService.GetByIdAsync(id);
            if (signature == null)
            {
                return NotFound();
            }

            signature.SortOrder = newSortOrder;
            var updatedSignature = await _signatureService.UpdateAsync(id, signature);
            return Ok(updatedSignature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering signature: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}



