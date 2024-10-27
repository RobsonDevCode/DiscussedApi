using System.ComponentModel.DataAnnotations;

namespace DiscussedApi.Models.Profiles
{
    public class Profile
    {
        [Key]
        [Required]
        public Guid UserId { get; set; }
        public int FollerCount { get; set; } = 0;
        public int FollowingCount { get; set; } = 0;

    }
}
