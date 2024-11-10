using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DiscussedApi.Configuration;
using Microsoft.Extensions.Options;
using DiscussedApi.Models.UserInfo;
using System.Reflection.Emit;

namespace DiscussedApi.Data.Identity
{
    public class ApplicationIdentityDBContext : IdentityDbContext<User>
    {
        public ApplicationIdentityDBContext(DbContextOptions<ApplicationIdentityDBContext> dbContextOptions) : base(dbContextOptions)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER",
                }

            };

            builder.Entity<IdentityRole>().HasData(roles);
        }

    }
}
