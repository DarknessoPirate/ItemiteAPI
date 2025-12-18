using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Banner
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [Required]
    public Dimensions Dimensions { get; set; }
    [Required]
    public Offset Offset { get; set; }
    [Required]
    public BannerPosition Position { get; set; }
    public bool IsActive { get; set; }
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; }
    [Required]
    public string Url { get; set; }
    [Required]
    public string PublicId { get; set; }
    [Required]
    public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
}