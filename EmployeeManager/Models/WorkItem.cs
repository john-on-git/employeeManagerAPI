using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models;

public class WorkItem : PerUserEntity
{
    /**
     * Description of the Category.
     */
    [MaxLength(1000)]
    public required string Description { get; init; }

    /**
     * Employee that the WorkItem is assigned to.
     * If this.WorkItemCategory is null, then accessing this value results in undefined behavior.
     */
    public required Employee? AssignedTo { get; init; }

    /**
     * WorkItemCategory that the WorkItem is associated with.
     * If null, then the WorkItem is in the implicit "unassigned" category.
     */
    public required WorkItemCategory? Category { get; init; }

    /**
     * <returns>True if the WorkItem meets the data constraints, otherwise false.</returns>
     */
    public bool IsValid()
    {
        // item is in the unassigned category ⇒ is not assigned to anyone 
        return Category != null || AssignedTo == null;
    }
}