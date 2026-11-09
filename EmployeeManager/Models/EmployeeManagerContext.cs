namespace EmployeeManager.Models;
using Microsoft.EntityFrameworkCore;

public class EmployeeManagerContext(DbContextOptions<EmployeeManagerContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Swimlane> Swimlanes { get; set; }
    public DbSet<Task> Tasks { get; set; }
}