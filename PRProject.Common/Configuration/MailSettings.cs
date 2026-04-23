namespace PRProject.Common.Configuration
{
    public class MailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }

        public string ImapHost { get; set; } = string.Empty;
        public int ImapPort { get; set; }

        public string Pop3Host { get; set; } = string.Empty;
        public int Pop3Port { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
    }
}