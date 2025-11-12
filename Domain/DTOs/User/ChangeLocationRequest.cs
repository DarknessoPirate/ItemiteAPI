using Domain.ValueObjects;

namespace Domain.DTOs.User;

public class ChangeLocationRequest
{
    public Location Location { get; set; }
}