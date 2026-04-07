using Microsoft.AspNetCore.Identity;

namespace Argus.Entities
{
    public class AppUser : IdentityUser
    {
        public List<Project> Projects { get; set; } = new();
    }
}
