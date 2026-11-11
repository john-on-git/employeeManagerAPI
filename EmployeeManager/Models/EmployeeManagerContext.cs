using Microsoft.IdentityModel.Protocols.Configuration;

namespace EmployeeManager.Models;
using Microsoft.EntityFrameworkCore;

public class EmployeeManagerContext : DbContext
{
    public EmployeeManagerContext(DbContextOptions options) : base(options) { }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<TaskCategory> TaskCategories { get; set; }
    public DbSet<Task> Tasks { get; set; }
}