using DiscussedApi.Configuration;
using DiscussedApi.Models.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiscussedApi.Data.UserComments
{
    public class CommentsDBContext : DbContext
    {
        public CommentsDBContext()
        {
        }

        public CommentsDBContext(DbContextOptions<CommentsDBContext> dbContextOptions) : base(dbContextOptions)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Settings.ConnectionString.UserInfo, ServerVersion.AutoDetect(Settings.ConnectionString.UserInfo));
            }
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replies { get; set; }
       
    }
}
