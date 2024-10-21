using DiscussedApi.Configuration;
using DiscussedApi.Models.Comments;
using Microsoft.EntityFrameworkCore;

namespace DiscussedApi.Data.UserComments
{
    public class CommentsDBContext : DbContext
    {
        public CommentsDBContext(DbContextOptions<CommentsDBContext> dbContextOptions) : base(dbContextOptions)
        {
            
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replys { get; set; }
       
    }
}
