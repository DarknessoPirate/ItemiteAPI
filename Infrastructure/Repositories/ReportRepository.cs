using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReportRepository(ItemiteDbContext context) : IReportRepository
{
    public async Task AddAsync(Report report)
    {
        await context.Reports.AddAsync(report);
    }

    public IQueryable<Report> GetReportsQueryable()
    {
        return context.Reports.Include(r => r.User)
            .Include(r => r.ReportPhotos).ThenInclude(rp => rp.Photo);
    }
}