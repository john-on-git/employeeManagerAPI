using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Identity.Web;

namespace EmployeeManager.Models.EF;

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
    public virtual int Id { get; init; }
    /**
     * ID of the Microsoft Identity Platform user managing this entity.
     */
    [MaxLength(1000)]
    [JsonIgnore]
    public virtual string? UserId { get; set; }
    
    /**
     * <returns>True if the user is authorized to access the entity, otherwise false.</returns>
     */
    public virtual bool VisibleToUser(ClaimsPrincipal user)
    {
        return UserId == user.GetObjectId();
    }
}