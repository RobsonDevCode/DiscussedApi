namespace DiscussedApi.Authenctication
{
    internal class JwtSettings
    {
        private IConfiguration _config { get; set; }
        internal JwtSettings(IConfiguration configuration) 
        {
            _config = configuration;
        }

        public string Issuer => _config.GetValue<string>($"JwtSettings:{nameof(Issuer)}") ?? 
            throw new NullReferenceException($"Trouble loading confiration for {nameof(Issuer)}");
        public string Audience => _config.GetValue<string>($"JwtSettings:{nameof(Audience)}") ??
           throw new NullReferenceException($"Trouble loading confiration for {nameof(Audience)}");
       
        public string Key => _config.GetValue<string>($"JwtSettings:{nameof(Key)}") ??
            throw new NullReferenceException($"Trouble loading confiration for {nameof(Key)}");


    }
}
