namespace EmployeeManager.Models;
using Microsoft.EntityFrameworkCore;

public class EmployeeManagerContext(DbContextOptions<EmployeeManagerContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<TaskCategory> TaskCategories { get; set; }
    public DbSet<Task> Tasks { get; set; }
}