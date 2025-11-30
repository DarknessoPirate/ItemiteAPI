namespace Domain.DTOs.Notifications;

public class PaginateNotificationsQuery
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;

    public override string ToString()
    {
        return $"{PageSize.ToString()}_" +
               $"{PageNumber.ToString()}";
    }
}