namespace Wuzzef.Application.DTOs.Response
{
    public class JobResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ApplicationsCount { get; set; }
    }
}
