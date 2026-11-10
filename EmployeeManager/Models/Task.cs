using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models;

public class Task
{
    /**
     * ID, PK.
     */
    [Key]
    public required int Id { get; init; }
    
    /**
     * Title of the Category.
     */
    [MaxLength(100)]
    public required string Title { get; init; }
    
    /**
     * Description of the Category.
     */
    [MaxLength(1000)]
    public required string Description { get; init; }
    
    /**
     * Employee that the Task is assigned to.
     */
    public required Employee Employee { get; init; }
    
    /**
     * TaskCategory that the Task is associated with.
     */
    public required TaskCategory Category { get; init; }
}