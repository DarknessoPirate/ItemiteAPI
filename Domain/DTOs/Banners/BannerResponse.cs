using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.DTOs.Banners;

public class BannerResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Dimensions Dimensions { get; set; }
    public Offset Offset { get; set; }
    public BannerPosition Position { get; set; }
    public bool IsActive { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public string PublicId { get; set; }
    public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
}