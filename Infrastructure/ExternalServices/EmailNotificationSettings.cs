namespace Infrastructure.ExternalServices
{
    public class EmailNotificationSettings
    {
        public bool Enabled { get; set; } = false;
        public string ResendApiKey { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string From { get; set; } = "MundoPreguica <onboarding@resend.dev>";
    }
}
