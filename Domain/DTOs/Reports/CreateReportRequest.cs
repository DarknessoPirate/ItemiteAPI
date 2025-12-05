using Domain.Enums;

namespace Domain.DTOs.Reports;

public class CreateReportRequest
{
    public string Content {get; set;}
    public ResourceType ResourceType {get; set;}
}