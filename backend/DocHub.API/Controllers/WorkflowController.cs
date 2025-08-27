using DocHub.Application.DTOs;
using DocHub.Core.Entities.Workflow;
using DocHub.Infrastructure.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILoggingService _loggingService;

    public WorkflowController(
        IWorkflowService workflowService,
        ILoggingService loggingService)
    {
        _workflowService = workflowService;
        _loggingService = loggingService;
    }

    [HttpPost("initialize")]
    public async Task<ActionResult<ApiResponse<WorkflowInstance>>> InitializeWorkflow(
        [FromQuery] string entityId,
        [FromQuery] string entityType)
    {
        try
        {
            var instance = await _workflowService.InitializeWorkflowAsync(entityId, entityType);
            return Ok(new ApiResponse<WorkflowInstance>(instance));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "InitializeWorkflow", ex.Message,
                new { entityId, entityType });
            return StatusCode(500, new ApiResponse<WorkflowInstance>(null, "Failed to initialize workflow"));
        }
    }

    [HttpPost("transition")]
    public async Task<ActionResult<ApiResponse<bool>>> TransitionToState(
        [FromQuery] string instanceId,
        [FromQuery] string toStateId,
        [FromBody] string? comments)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _workflowService.TransitionToStateAsync(instanceId, toStateId, userId, comments);

            if (!result)
                return BadRequest(new ApiResponse<bool>(false, "Invalid transition"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "TransitionToState", ex.Message,
                new { instanceId, toStateId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to transition state"));
        }
    }

    [HttpPost("approvals")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateApprovalRequest(
        [FromQuery] string instanceId,
        [FromQuery] string transitionId,
        [FromQuery] string approverId,
        [FromQuery] string approverType)
    {
        try
        {
            var result = await _workflowService.CreateApprovalRequestAsync(instanceId, transitionId, approverId, approverType);
            return Ok(new ApiResponse<bool>(result));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "CreateApprovalRequest", ex.Message,
                new { instanceId, transitionId, approverId, approverType });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to create approval request"));
        }
    }

    [HttpPost("approvals/{approvalId}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveTransition(
        string approvalId,
        [FromBody] string? comments)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _workflowService.ApproveTransitionAsync(approvalId, userId, comments);

            if (!result)
                return BadRequest(new ApiResponse<bool>(false, "Invalid approval"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "ApproveTransition", ex.Message,
                new { approvalId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to approve transition"));
        }
    }

    [HttpPost("approvals/{approvalId}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectTransition(
        string approvalId,
        [FromBody] string? comments)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _workflowService.RejectTransitionAsync(approvalId, userId, comments);

            if (!result)
                return BadRequest(new ApiResponse<bool>(false, "Invalid rejection"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "RejectTransition", ex.Message,
                new { approvalId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to reject transition"));
        }
    }

    [HttpGet("{instanceId}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkflowHistory>>>> GetWorkflowHistory(string instanceId)
    {
        try
        {
            var history = await _workflowService.GetWorkflowHistoryAsync(instanceId);
            return Ok(new ApiResponse<IEnumerable<WorkflowHistory>>(history));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "GetWorkflowHistory", ex.Message,
                new { instanceId });
            return StatusCode(500, new ApiResponse<IEnumerable<WorkflowHistory>>(null, "Failed to retrieve workflow history"));
        }
    }

    [HttpGet("approvals/pending")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkflowApproval>>>> GetPendingApprovals(
        [FromQuery] string approverId,
        [FromQuery] string approverType)
    {
        try
        {
            var approvals = await _workflowService.GetPendingApprovalsAsync(approverId, approverType);
            return Ok(new ApiResponse<IEnumerable<WorkflowApproval>>(approvals));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "GetPendingApprovals", ex.Message,
                new { approverId, approverType });
            return StatusCode(500, new ApiResponse<IEnumerable<WorkflowApproval>>(null, "Failed to retrieve pending approvals"));
        }
    }

    [HttpGet("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateTransition(
        [FromQuery] string instanceId,
        [FromQuery] string toStateId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var isValid = await _workflowService.ValidateTransitionAsync(instanceId, toStateId, userId);
            return Ok(new ApiResponse<bool>(isValid));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "ValidateTransition", ex.Message,
                new { instanceId, toStateId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to validate transition"));
        }
    }
}
