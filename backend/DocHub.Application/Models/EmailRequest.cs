namespace DocHub.Application.Models
{
    public class EmailRequest
    {
        public string To { get; set; }
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<string> AttachmentPaths { get; set; } = new();
    }
}
