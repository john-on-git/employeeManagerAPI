using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Models;

[PrimaryKey(nameof(Id), nameof(UserId))]
public class TaskCategory
{
    /**
     * Component of the composite key. Unique per user.
     */
    [Column(Order = 0)]
    public required int Id { get; init; }
    
    /**
     * ID of the Microsoft Identity Platform user owning this TaskCategory.
     * Component of the composite key.
     */
    [Column(Order = 1)]
    [MaxLength(1000)]
    public required string UserId { get; init; }
    
    /**
     * Name of the Category.
     */
    [MaxLength(100)]
    public required string Name { get; init; }
    
    /**
     * Tasks in the Category.
     */
    public required ICollection<Task> Tasks { get; init; }
}