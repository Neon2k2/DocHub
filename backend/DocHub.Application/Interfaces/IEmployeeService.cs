using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee> GetByIdAsync(string id);
    Task<Employee> GetByEmployeeIdAsync(string employeeId);
    Task<Employee> GetByEmailAsync(string email);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(string id, Employee employee);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);
    Task<IEnumerable<Employee>> GetByDesignationAsync(string designation);
    Task<bool> ExistsAsync(string employeeId);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<Employee>> GetPagedAsync(int page, int pageSize);
}
