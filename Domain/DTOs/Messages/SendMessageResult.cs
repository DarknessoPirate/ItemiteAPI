using Domain.DTOs.Photo;

namespace Domain.DTOs.Messages;

public class SendMessageResult
{
    public MessageResponse message { get; set; }
    public List<PhotoUploadResult> UploadResults { get; set; } = [];
}