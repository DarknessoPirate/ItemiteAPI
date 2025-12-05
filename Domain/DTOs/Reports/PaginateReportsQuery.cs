using Domain.Enums;

namespace Domain.DTOs.Reports;

public class PaginateReportsQuery
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public ResourceType? ResourceType { get; set; }
}