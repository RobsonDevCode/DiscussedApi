using DiscussedApi.Configuration;
using DiscussedApi.Models.Profiles;
using Microsoft.EntityFrameworkCore;

namespace DiscussedApi.Data.Profiles
{
    public class ProfileDBContext : DbContext 
    {
        public ProfileDBContext()
        {
            
        }
        public ProfileDBContext(DbContextOptions<ProfileDBContext> dbContextOptions) : base(dbContextOptions) 
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Settings.ConnectionString.UserInfo, ServerVersion.AutoDetect(Settings.ConnectionString.UserInfo));
            }

        }

        public DbSet<Profile> Profile { get; set; }
        public DbSet<Follower> Follower { get; set; }
        public DbSet<Following> Following { get; set; }
    }
}
