using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Models;

[PrimaryKey(nameof(Id), nameof(UserId))]
public class Employee
{
    /**
     * Component of the composite key. Unique per user.
     */
    [Column(Order = 0)]
    public required int Id { get; init; }
    
    /**
     * ID of the Microsoft Identity Platform user managing this Employee.
     * Component of the composite key.
     */
    [Column(Order = 1)]
    [MaxLength(1000)]
    public required string UserId { get; init; }
    
    /**
     * Full name of the Employee.
     */
    [MaxLength(100)]
    public required string Name { get; init; }
    
    /**
     * Additional notes / info about the employee.
     */
    [MaxLength(1000)]
    public required string EmployeeNotes { get; init; }
    
    /**
     * Tasks assigned to the Employee.
     */
    public required ICollection<Task> Tasks { get; init; }
}