using DocHub.Core.Entities.Authorization;

namespace DocHub.Core.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    // Add role-specific repository methods here
}

public interface IPermissionRepository : IGenericRepository<Permission>
{
    // Add permission-specific repository methods here
}

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
    // Add role-permission-specific repository methods here
}

public interface IUserRoleRepository : IGenericRepository<UserRole>
{
    // Add user-role-specific repository methods here
}
