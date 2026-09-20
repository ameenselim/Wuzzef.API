namespace Wuzzef.Application.DTOs.Response
{
    public class ApplicationResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
    }
}
