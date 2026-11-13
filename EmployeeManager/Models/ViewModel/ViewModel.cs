using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using EmployeeManager.Models.EF;
using Microsoft.Identity.Web;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
public abstract class ViewModel<T>(int id) where T : PerUserEntity
{
    public virtual required int Id { get; init; } = id;

    public T ToEFModel(ClaimsPrincipal user)
    {
        return ToEFModel(user.GetObjectId());
    }
    public abstract T ToEFModel(string? userId);
}