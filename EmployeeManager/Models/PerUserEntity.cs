using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeManager.Models;

public abstract class PerUserEntity
{
    /* TODO, ideally this would use a composite key.
     * It would make the IDs per-user and we it would make fetching rows easier.
     * However EF can't seem to handle this and generates duplicate PKs.
     */
    
    /**
     * Unique ID.
     */
    [Key]
    public int Id { get; init; }
    /**
     * ID of the Microsoft Identity Platform user managing this entity.
     */
    [MaxLength(1000)]
    [JsonIgnore]
    public string? UserId { get; set; }
}