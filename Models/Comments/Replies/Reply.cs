﻿using System.ComponentModel.DataAnnotations;

namespace DiscussedApi.Models.Comments.Replies
{
    public class Reply
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Likes { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime DtUpdated { get; set; }

    }
}