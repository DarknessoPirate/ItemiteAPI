using Domain.Enums;

namespace Domain.Entities;

public class Report
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ResourceType? ResourceType { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }

    public ICollection<ReportPhoto> ReportPhotos { get; set; } = [];
}