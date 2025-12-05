using Domain.DTOs.Pagination;
using Domain.DTOs.Reports;
using MediatR;

namespace Application.Features.Reports.GetPaginatedReports;

public class GetPaginatedReportsQuery : IRequest<PageResponse<ReportResponse>>
{
    public PaginateReportsQuery Query { get; set; }
}