using Domain.DTOs.User;
using Domain.Enums;

namespace Domain.DTOs.Reports;

public class ReportResponse
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public ResourceType? ReportedResource { get; set; }
    public UserBasicResponse ReportSubmitter { get; set; }
    public ICollection<string> ImagesUrls { get; set; } = [];
}