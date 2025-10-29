namespace Domain.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string Url { get; set; }
    public required string PublicId { get; set; }
    public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
}