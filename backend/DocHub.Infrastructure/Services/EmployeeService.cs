using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using DocHub.Infrastructure.Repositories;
using DocHub.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly DocHubDbContext _context;
    private readonly IGenericRepository<Employee> _repository;

    public EmployeeService(DocHubDbContext context, IGenericRepository<Employee> repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<Employee> GetByIdAsync(string id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<Employee> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<Employee> GetByEmailAsync(string email)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        if (await ExistsAsync(employee.EmployeeId))
        {
            throw new InvalidOperationException($"Employee with ID '{employee.EmployeeId}' already exists.");
        }

        if (await EmailExistsAsync(employee.Email))
        {
            throw new InvalidOperationException($"Employee with email '{employee.Email}' already exists.");
        }

        employee.Id = Guid.NewGuid().ToString();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.AddAsync(employee);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<Employee> UpdateAsync(string id, Employee employee)
    {
        var existingEmployee = await GetByIdAsync(id);
        if (existingEmployee == null)
        {
            throw new InvalidOperationException($"Employee with id '{id}' not found.");
        }

        // Check if employee ID is being changed and if it already exists
        if (employee.EmployeeId != existingEmployee.EmployeeId && await ExistsAsync(employee.EmployeeId))
        {
            throw new InvalidOperationException($"Employee with ID '{employee.EmployeeId}' already exists.");
        }

        // Check if email is being changed and if it already exists
        if (employee.Email != existingEmployee.Email && await EmailExistsAsync(employee.Email))
        {
            throw new InvalidOperationException($"Employee with email '{employee.Email}' already exists.");
        }

        existingEmployee.EmployeeId = employee.EmployeeId;
        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.MiddleName = employee.MiddleName;
        existingEmployee.Email = employee.Email;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Department = employee.Department;
        existingEmployee.Designation = employee.Designation;
        existingEmployee.IsActive = employee.IsActive;
        existingEmployee.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingEmployee);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var employee = await GetByIdAsync(id);
        if (employee == null)
        {
            return false;
        }

        // Check if employee has generated letters
        var hasGeneratedLetters = await _context.GeneratedLetters
            .AnyAsync(gl => gl.EmployeeId == id);

        if (hasGeneratedLetters)
        {
            throw new InvalidOperationException("Cannot delete employee that has generated letters.");
        }

        var result = await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
    {
        return await _context.Employees
            .Where(e => e.Department == department && e.IsActive)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByDesignationAsync(string designation)
    {
        return await _context.Employees
            .Where(e => e.Designation == designation && e.IsActive)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string employeeId)
    {
        return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Employees.AnyAsync(e => e.Email == email);
    }

    public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var term = searchTerm.ToLower();
        return await _context.Employees
            .Where(e => e.IsActive && (
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.EmployeeId.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.Department.ToLower().Contains(term) ||
                e.Designation.ToLower().Contains(term)
            ))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Employees.CountAsync();
    }

    public async Task<IEnumerable<Employee>> GetPagedAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return await _context.Employees
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }
}
