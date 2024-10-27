using Microsoft.AspNetCore.Identity;

namespace DiscussedApi.Models.UserInfo
{
    public class User : IdentityUser
    {
        public int Followers { get; set; }
        public int Following { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
