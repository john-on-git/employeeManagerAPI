using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Identity.Web;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
[method: JsonConstructor]
public class WorkItemViewModel(int id, string title, string description) : ViewModel<WorkItem>(id)
{
    [method: SetsRequiredMembers]
    public WorkItemViewModel(WorkItem workItem) : this(workItem.Id, workItem.Title, workItem.Description)
    {
    }
    
    [MaxLength(100)]
    public required string Title { get; init; } = title;
    [MaxLength(1000)]
    public required string Description { get; init; } = description;
    
    public override WorkItem ToEFModel(ClaimsPrincipal user)
    {
        return new WorkItem
        {
            Id=Id,
            UserId=user.GetObjectId(),
            Title=Title,
            Description=Description,
            AssignedTo=null,
            Category=null
        };
    }
}