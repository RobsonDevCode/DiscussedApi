using System.ComponentModel.DataAnnotations;

namespace DiscussedApi.Models.Profiles
{
    public class Following
    {
        [Key]
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public Guid? UserFollowing { get; set; }
        public bool? IsFollowing { get; set; }

    }
}
