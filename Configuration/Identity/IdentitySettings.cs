using Newtonsoft.Json;

namespace DiscussedApi.Configuration.Identity
{
    internal class IdentitySettings
    {
        private readonly IConfiguration _config;
        internal IdentitySettings(IConfiguration config)
        {
            _config = config;
        }
        public AllowedCharacters AllowedChars => new AllowedCharacters(_config);
    }
}
