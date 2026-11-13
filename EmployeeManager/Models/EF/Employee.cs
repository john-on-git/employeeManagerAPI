using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EmployeeManager.Models.EF;


[method: SetsRequiredMembers]
public class Employee : PerUserEntity
{
    /**
     * Full name of the Employee.
     */
    [MaxLength(100)]
    public virtual required string Name { get; init; }

    /**
     * Additional notes / info about the employee.
     */
    [MaxLength(1000)]
    public virtual required string Notes { get; init; }

    /**
     * Tasks assigned to the Employee.
     */
    public virtual ICollection<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}