using DiscussedApi.Authenctication;
using DiscussedApi.Configuration.Identity;
using Newtonsoft.Json;

namespace DiscussedApi.Configuration
{
    internal static class Settings
    {
        private static IConfiguration _config { get; set; }

        internal static void Initialize(IConfiguration configuration)
        {
            _config = configuration;
        }


        public static ConnectionStrings ConnectionString => new ConnectionStrings(_config);
        public static JwtSettings JwtSettings => new JwtSettings(_config);
        public static EmailSettings EmailSettings => new EmailSettings(_config);
        public static IdentitySettings  IdentitySettings => new IdentitySettings(_config);

        public static int ParallelWorkers = 4;

        public static int CommentMax = 100; 

    }
}
