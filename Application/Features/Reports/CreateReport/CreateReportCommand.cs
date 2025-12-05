using Domain.DTOs.File;
using Domain.DTOs.Reports;
using MediatR;

namespace Application.Features.Reports.CreateReport;

public class CreateReportCommand : IRequest<int>
{
    public int UserId { get; set; }
    public List<FileWrapper>? Images {get; set;}
    public CreateReportRequest ReportDto {get; set;}
}