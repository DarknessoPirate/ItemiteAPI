namespace Domain.DTOs.Photo;

public class PhotoUploadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; } 
    public string? ErrorMessage { get; set; }  
}
