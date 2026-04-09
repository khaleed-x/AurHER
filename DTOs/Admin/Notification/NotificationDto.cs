namespace AurHER.DTOs.Admin
{
    public class EmailNotificationDto
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SmsNotificationDto
    {
        public string To { get; set; }
        public string Message { get; set; }
    }
}