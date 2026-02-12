namespace Infrastructure.ExternalServices
{
    public class EmailNotificationSettings
    {
        public bool Enabled { get; set; } = false;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
