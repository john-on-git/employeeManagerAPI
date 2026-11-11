using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManager.Models;

public class Task
{
    /**
     * Component of the composite key. Unique per user.
     */
    [Key]
    public required int Id { get; init; }

    /**
     * Description of the Category.
     */
    [MaxLength(1000)]
    public required string Description { get; init; }
    
    /**
     * Employee that the Task is assigned to.
     */
    public required Employee? Employee { get; init; }
    
    /**
     * TaskCategory that the Task is associated with.
     */
    public required TaskCategory Category { get; init; }
}