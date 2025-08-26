using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IEmployeeService employeeService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var employees = await _employeeService.GetPagedAsync(page, pageSize);
            var totalCount = await _employeeService.GetTotalCountAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-PageSize", pageSize.ToString());

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all employees");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Employee>>> Search([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search term is required");
            }

            var employees = await _employeeService.SearchAsync(q);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with term: {SearchTerm}", q);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("department/{department}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetByDepartment(string department)
    {
        try
        {
            var employees = await _employeeService.GetByDepartmentAsync(department);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by department: {Department}", department);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("designation/{designation}")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetByDesignation(string designation)
    {
        try
        {
            var employees = await _employeeService.GetByDesignationAsync(designation);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by designation: {Designation}", designation);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetById(string id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("employee-id/{employeeId}")]
    public async Task<ActionResult<Employee>> GetByEmployeeId(string employeeId)
    {
        try
        {
            var employee = await _employeeService.GetByEmployeeIdAsync(employeeId);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by employee ID: {EmployeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<Employee>> GetByEmail(string email)
    {
        try
        {
            var employee = await _employeeService.GetByEmailAsync(email);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by email: {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> Create([FromBody] Employee employee)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEmployee = await _employeeService.CreateAsync(employee);
            return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<Employee>>> CreateBulk([FromBody] List<Employee> employees)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (employees == null || !employees.Any())
            {
                return BadRequest("Employee list is required");
            }

            var createdEmployees = new List<Employee>();
            foreach (var employee in employees)
            {
                try
                {
                    var createdEmployee = await _employeeService.CreateAsync(employee);
                    createdEmployees.Add(createdEmployee);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Error creating employee: {EmployeeId}", employee.EmployeeId);
                    // Continue with other employees
                }
            }

            return CreatedAtAction(nameof(GetAll), createdEmployees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk employees");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Employee>> Update(string id, [FromBody] Employee employee)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedEmployee = await _employeeService.UpdateAsync(id, employee);
            return Ok(updatedEmployee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var result = await _employeeService.DeleteAsync(id);
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
            _logger.LogError(ex, "Error deleting employee: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(string id)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = !employee.IsActive;
            var updatedEmployee = await _employeeService.UpdateAsync(id, employee);
            return Ok(updatedEmployee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling employee status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
