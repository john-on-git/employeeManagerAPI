using Microsoft.IdentityModel.Protocols.Configuration;

namespace EmployeeManager.Models;
using Microsoft.EntityFrameworkCore;

public class EmployeeManagerContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<WorkItemCategory> WorkItemCategories { get; set; }
    public DbSet<WorkItem> WorkItems { get; set; }
}