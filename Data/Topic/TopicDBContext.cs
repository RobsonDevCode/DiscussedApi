using DiscussedApi.Configuration;
using DiscussedApi.Data.Profiles;
using Microsoft.EntityFrameworkCore;
using DiscussedApi.Models.Topic;

namespace DiscussedApi.Data.Topics
{
    public class TopicDBContext : DbContext
    {
        public TopicDBContext()
        {

        }
        public TopicDBContext(DbContextOptions<TopicDBContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Settings.ConnectionString.UserInfo, ServerVersion.AutoDetect(Settings.ConnectionString.UserInfo));
            }

        }

        public DbSet<Topic> Topics { get; set; }

    }
}
