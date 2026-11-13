using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using EmployeeManager.Models.EF;
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
    public virtual required string Title { get; init; } = title;
    [MaxLength(1000)]
    public virtual required string Description { get; init; } = description;
    
    public override WorkItem ToEFModel(string? userId)
    {
        return new WorkItem
        {
            Id=Id,
            UserId=userId,
            Title=Title,
            Description=Description,
            AssignedTo=null,
            Category=null
        };
    }
}