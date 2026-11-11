using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EmployeeManager.Models;

public class WorkItemCategory : PerUserEntity
{
    /**
     * Name of the Category.
     */
    [MaxLength(100)]
    public required string Name { get; init; }

    /**
     * Tasks in the Category.
     */
    public ICollection<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}