using Microsoft.AspNetCore.Identity;

namespace DiscussedApi.Models
{
    public class User : IdentityUser
    {
        public int Followers { get; set; }
        public int Following {  get; set; }
    }
}
