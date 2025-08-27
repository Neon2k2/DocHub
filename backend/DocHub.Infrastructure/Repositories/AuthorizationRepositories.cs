using DocHub.Core.Entities.Authorization;
using DocHub.Core.Interfaces.Repositories;

namespace DocHub.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Add role-specific repository methods here
}

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Add permission-specific repository methods here
}

public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RolePermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Add role-permission-specific repository methods here
}

public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Add user-role-specific repository methods here
}
