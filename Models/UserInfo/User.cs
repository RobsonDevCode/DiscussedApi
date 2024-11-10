using Microsoft.AspNetCore.Identity;

namespace DiscussedApi.Models.UserInfo
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
