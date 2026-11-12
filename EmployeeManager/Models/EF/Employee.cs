using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EmployeeManager.Models;

[method: SetsRequiredMembers]
public class Employee : PerUserEntity
{
    /**
     * Full name of the Employee.
     */
    [MaxLength(100)]
    public required string Name { get; init; }

    /**
     * Additional notes / info about the employee.
     */
    [MaxLength(1000)]
    public required string Notes { get; init; }

    /**
     * Tasks assigned to the Employee.
     */
    public ICollection<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}