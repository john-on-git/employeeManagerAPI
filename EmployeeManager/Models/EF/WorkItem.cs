using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeManager.Models.EF;

public class WorkItem : PerUserEntity
{
    /**
     * Tile for the work item.
     */
    [MinLength(1), MaxLength(100)]
    public virtual required string Title { get; init; }
    /**
     * Description of the work item.
     */
    [MinLength(1), MaxLength(1000)]
    public virtual required string Description { get; init; }

    /**
     * FK for Employee.
     */
    public virtual required int? AssignedToId { get; init; }
    /**
     * Employee that the WorkItem is assigned to.
     * If this.WorkItemCategory is null, then accessing this value results in undefined behavior.
     */
    public virtual required Employee? AssignedTo { get; init; }

    /**
     * FK for WorkItemCategory.
     */
    public virtual required int? CategoryId { get; init; }
    /**
     * WorkItemCategory that the WorkItem is associated with.
     * If null, then the WorkItem is in the implicit "unassigned" category.
     */
    public virtual required WorkItemCategory? Category { get; init; }

    /**
     * <returns>True if the WorkItem meets the data constraints, otherwise false.</returns>
     */
    public virtual bool IsValid()
    {
        // item is in the unassigned category ⇒ is not assigned to anyone 
        return Category != null || AssignedTo == null;
    }
}