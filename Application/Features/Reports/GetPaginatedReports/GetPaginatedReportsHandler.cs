using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.Reports;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.GetPaginatedReports;

public class GetPaginatedReportsHandler(
    IReportRepository reportRepository,
    IMapper mapper
    ) : IRequestHandler<GetPaginatedReportsQuery, PageResponse<ReportResponse>>
{
    public async Task<PageResponse<ReportResponse>> Handle(GetPaginatedReportsQuery request, CancellationToken cancellationToken)
    {
        var reportsQuery = reportRepository.GetReportsQueryable();
        
        
        int totalItems = await reportsQuery.CountAsync(cancellationToken);

        if (request.Query.ResourceType != null)
        {
            reportsQuery = reportsQuery.Where(r => r.ResourceType == request.Query.ResourceType);
        }

        reportsQuery = reportsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize);
        
        var mappedReports = mapper.Map<List<ReportResponse>>(await reportsQuery.ToListAsync());
        
        return new PageResponse<ReportResponse>(mappedReports, totalItems, request.Query.PageSize, request.Query.PageNumber);
    }
}