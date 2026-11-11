using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Models;

[PrimaryKey(nameof(Id), nameof(UserId))]
public abstract class PerUserEntity
{
    /**
     * Component of the composite key. Unique per user.
     */
    [Key]
    public int Id { get; init; }
    /**
     * ID of the Microsoft Identity Platform user managing this entity.
     * Component of the composite key.
     */
    [Key, MaxLength(1000)]
    [JsonIgnore]
    public string? UserId { get; set; }
}