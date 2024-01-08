using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain;
public sealed class Role:IdentityRole<Guid>
{
  public Role()
  {
    this.Id = Guid.NewGuid();
  }
}