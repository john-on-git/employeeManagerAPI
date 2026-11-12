using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.Identity.Web;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
public class EmployeeViewModel(int id, string name, string notes) : ViewModel<Employee>(id)
{
    [method: SetsRequiredMembers]
    public EmployeeViewModel(Employee employee) : this(employee.Id, employee.Name, employee.Notes)
    {
    }
    
    [MaxLength(100)]
    public required string Name { get; init; } = name;
    [MaxLength(1000)]
    public required string Notes { get; init; } = notes;
    
    public override Employee ToEFModel(ClaimsPrincipal user)
    {
        return new Employee
        {
            Id=Id, 
            UserId=user.GetObjectId(),
            Name=Name,
            Notes=Notes
        };
    }
}