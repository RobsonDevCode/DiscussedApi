
namespace PFMSApi.Configuration
{
    internal class ConnectionStrings
    {
        private IConfiguration _config { get; set; }

        internal ConnectionStrings(IConfiguration config)
        {
            this._config = config;
        }

        public string UserInfo => _config.GetConnectionString(nameof(UserInfo)) ?? 
            throw new NullReferenceException($"Connection for {nameof(UserInfo)} hasnt been configured correctly");

    }
}