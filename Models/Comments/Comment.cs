using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscussedApi.Models.Comments
{
    public class Comment
    {

        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Context { get; set; } = string.Empty;
        public int ReplyCount { get; set; }
        public int Likes { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime DtUpdated { get; set; }
    }
}
