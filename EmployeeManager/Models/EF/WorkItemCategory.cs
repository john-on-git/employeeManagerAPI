using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Models.EF;

public class WorkItemCategory : PerUserEntity
{
    public const string DefaultCategoryName = "DEFAULT_CATEGORY";

    /**
     * Name of the Category.
     */
    [MinLength(1), MaxLength(100)]
    public virtual required string Name { get; init; }

    /**
     * Tasks in the Category.
     */
    public virtual ICollection<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}