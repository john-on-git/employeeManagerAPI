using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EmployeeManager.Models.EF;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
[method: JsonConstructor]
public class WorkItemViewModel(int id, string title, string description, int? assignedToId, int? categoryId) : ViewModel<WorkItem>(id)
{
    [method: SetsRequiredMembers]
    public WorkItemViewModel(WorkItem workItem) : this(workItem.Id, workItem.Title, workItem.Description, workItem.AssignedToId, workItem.CategoryId)
    {
    }
    
    [MaxLength(100)]
    public virtual required string Title { get; init; } = title;
    [MaxLength(1000)]
    public virtual required string Description { get; init; } = description;

    public virtual required int? AssignedToId { get; init; } = assignedToId;

    public virtual required int? CategoryId { get; init; } = categoryId;
    
    public override WorkItem ToEFModel(string? userId)
    {
        return new WorkItem
        {
            Id=Id,
            UserId=userId,
            Title=Title,
            Description=Description,
            AssignedToId = AssignedToId,
            AssignedTo=null,
            CategoryId = CategoryId,
            Category=null
        };
    }
}