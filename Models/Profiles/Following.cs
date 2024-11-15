using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscussedApi.Models.Profiles
{
    public class Following
    {
  
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public Guid? UserFollowing { get; set; }
        public bool? IsFollowing { get; set; }

        [Key]
        public long? FollowRef { get; set; } = 0;

    }
}
