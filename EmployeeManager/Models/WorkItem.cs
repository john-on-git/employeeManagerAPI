using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models;

public class WorkItem
{
    /**
     * Unique id.
     */
    [Key]
    public int Id { get; init; }

    /**
     * Description of the Category.
     */
    [MaxLength(1000)]
    public required string Description { get; init; }

    /**
     * Employee that the Task is assigned to.
     */
    public required Employee? AssignedTo { get; init; }

    /**
     * TaskCategory that the Task is associated with.
     */
    public required WorkItemCategory Category { get; init; }
}