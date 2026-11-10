using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models;

public class TaskCategory
{
    /**
     * Component of the composite key. Unique per user.
     */
    [Key]
    public required int Id { get; init; }
    
    /**
     * ID of the Microsoft Identity Platform user owning this TaskCategory.
     * Component of the composite key.
     */
    [Key]
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