namespace DiscussedApi.Configuration
{
    internal class EmailSettings
    {
        private readonly IConfiguration _config;
        internal EmailSettings(IConfiguration config) 
        {
            this._config = config;
        }

        public string Key => _config.GetValue<string>($"EmailSenderSetting:{nameof(Key)}") ??
           throw new NullReferenceException($"Value for {nameof(Key)} hasnt been configured correctly");

        public string Secret => _config.GetValue<string>($"EmailSenderSetting:{nameof(Secret)}") ??
            throw new NullReferenceException($"Value for {nameof(Secret)} hasnt been configured correctly");
        public string Sender => _config.GetValue<string>($"EmailSenderSetting:{nameof(Sender)}") ??
            throw new NullReferenceException($"Value for {nameof(Secret)} hasnt been configured correctly");

        public string RecoveryHtmlBodyFilePath => _config.GetValue<string>($"EmailSenderSetting:{nameof(RecoveryHtmlBodyFilePath)}") ??
         throw new NullReferenceException($"Value for {nameof(RecoveryHtmlBodyFilePath)} hasnt been configured correctly");

        public string ConfirmationBodyFilePath => _config.GetValue<string>($"EmailSenderSetting:{nameof(ConfirmationBodyFilePath)}") ??
         throw new NullReferenceException($"Value for {nameof(ConfirmationBodyFilePath)} hasnt been configured correctly");

        public string ConfirmationSubject = "Account Confirmation";

        public string RecoverySubject = "Recover Account";
    }
}