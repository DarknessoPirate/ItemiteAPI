using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.DTOs.Banners;

public class AddBannerRequest
{
    public string Name { get; set; }
    public Offset Offset { get; set; }
    public BannerPosition Position { get; set; }
    public bool IsActive { get; set; }
}