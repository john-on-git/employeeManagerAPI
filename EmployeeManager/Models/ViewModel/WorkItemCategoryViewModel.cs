using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using EmployeeManager.Models.EF;
using Microsoft.Identity.Web;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
public class WorkItemCategoryViewModel(int id, string name, ICollection<WorkItem> workItems) : ViewModel<WorkItemCategory>(id)
{
    
    [method: SetsRequiredMembers]
    public WorkItemCategoryViewModel(WorkItemCategory category) : this(category.Id, category.Name, category.WorkItems)
    {
    }
    
    /**
     * Name of the Category.
     */
    [MaxLength(100)]
    public required string Name { get; init; } = name;

    /**
     * Tasks in the Category.
     */
    public required ICollection<WorkItemViewModel> WorkItems { get; init; } = [..workItems.Select(x => new WorkItemViewModel(x))];

    public override WorkItemCategory ToEFModel(string? userId)
    {
        return new WorkItemCategory()
        {
            Id=Id,
            UserId=userId,
            Name=Name,
            WorkItems=[.. WorkItems.Select(x => x.ToEFModel(userId))]
        };
    }
}