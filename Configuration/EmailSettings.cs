namespace PFMSApi.Configuration
{
    internal class EmailSettings
    {
        private readonly IConfiguration _config;
        internal EmailSettings(IConfiguration config) 
        {
            this._config = config;
        }

        public string Mailer => _config.GetValue<string>($"EmailSenderSetting:{nameof(Mailer)}") ??
           throw new NullReferenceException($"Value for {nameof(Mailer)} hasnt been configured correctly");

        public string Password => _config.GetValue<string>($"EmailSenderSetting:{nameof(Password)}") ??
            throw new NullReferenceException($"Value for {nameof(Password)} hasnt been configured correctly");

        public string RecoveryHtmlBodyFilePath => _config.GetValue<string>($"EmailSenderSetting:{nameof(RecoveryHtmlBodyFilePath)}") ??
         throw new NullReferenceException($"Value for {nameof(RecoveryHtmlBodyFilePath)} hasnt been configured correctly");

        public string ConfirmationBodyFilePath => _config.GetValue<string>($"EmailSenderSetting:{nameof(ConfirmationBodyFilePath)}") ??
         throw new NullReferenceException($"Value for {nameof(ConfirmationBodyFilePath)} hasnt been configured correctly");

    }
}