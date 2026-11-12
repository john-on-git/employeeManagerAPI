using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Identity.Web;

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
    public int Id { get; set; }
    /**
     * ID of the Microsoft Identity Platform user managing this entity.
     */
    [MaxLength(1000)]
    [JsonIgnore]
    public string? UserId { get; set; }
    
    /**
     * <returns>True if the user is authorized to access the entity, otherwise false.</returns>
     */
    public bool VisibleToUser(ClaimsPrincipal user)
    {
        return UserId != user.GetObjectId();
    }
}