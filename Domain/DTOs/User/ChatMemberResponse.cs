namespace Domain.DTOs.User;

public class ChatMemberResponse
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string PhotoUrl { get; set; }
    public bool ListingOwner { get; set; }
}