using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DiscussedApi.Models.Comments
{
    public class Comment
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        [Key]
        public long Reference { get; set; }

        public string TopicId { get; set; } = string.Empty;
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
        public long Interactions { get; set; } 
        public int ReplyCount { get; set; }
        public int Likes { get; set; } 
        public DateTime DtCreated { get; set; }
        public DateTime DtUpdated { get; set; }
    }
}
