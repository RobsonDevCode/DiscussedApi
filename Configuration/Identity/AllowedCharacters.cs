using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DiscussedApi.Configuration.Identity
{
    internal class AllowedCharacters
    {
        private readonly IConfiguration _config; 

        internal AllowedCharacters(IConfiguration config)
        {
            _config = config;
        }

        public string DefaultAscii => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(DefaultAscii)}") 
            ?? throw new NullReferenceException($"Value for {nameof(DefaultAscii)} hasnt been configured correctly");

        public string LatinExtended => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(LatinExtended)}")
            ?? throw new NullReferenceException($"Value for {nameof( LatinExtended)} hasnt been configured correctly");
        public string Japanese => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(Japanese)}")
         ?? throw new NullReferenceException($"Value for {nameof(Japanese)} hasnt been configured correctly");
        //Not active
        public string Cyrilli => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(Cyrilli)}")
            ?? throw new NullReferenceException($"Value for {nameof(Cyrilli)} hasnt been configured correctly");
        //Not active

        public string Greek => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(Greek)}")
            ?? throw new NullReferenceException($"Value for {nameof(Greek)} hasnt been configured correctly");
        //Not active

        public string Arabic => _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(Arabic)}")
            ?? throw new NullReferenceException($"Value for {nameof(Arabic)} hasnt been configured correctly");
        //Not active

        public string Chinese=> _config.GetValue<string>($"IdentitySettings:AllowedCharacters:{nameof(Chinese)}")
            ?? throw new NullReferenceException($"Value for {nameof(Chinese)} hasnt been configured correctly");

     
    }
}