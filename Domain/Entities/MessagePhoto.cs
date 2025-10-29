namespace Domain.Entities;

public class MessagePhoto
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message Message { get; set; }
    public int PhotoId { get; set; }
    public Photo Photo { get; set; }
}