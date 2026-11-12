using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace EmployeeManager.Models.ViewModel;

[method: SetsRequiredMembers]
public abstract class ViewModel<T>(int id) where T : PerUserEntity
{
    public required int Id { get; init; } = id;

    public abstract T ToEFModel(ClaimsPrincipal user);
}