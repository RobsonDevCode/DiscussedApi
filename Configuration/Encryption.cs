
using System.Text;

namespace DiscussedApi.Configuration
{
    internal class Encryption
    {
        private IConfiguration _config;

        public Encryption(IConfiguration config)
        {
            this._config = config;
        }

        public byte[] Key => Encoding.UTF8.GetBytes(_config.GetValue<string>($"Encryption:{nameof(Key)}")
            ?? throw new NullReferenceException($"Value for {nameof(Key)} hasnt been configured correctly"));

        public byte[] IV => Encoding.UTF8.GetBytes(_config.GetValue<string>($"Encryption:{nameof(IV)}")
            ?? throw new NullReferenceException($"Value for {nameof(IV  )} hasnt been configured correctly"));

        public readonly int PasswordResetExpireTime = 15; //minutes 
    }
}