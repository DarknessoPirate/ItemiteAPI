using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IReportRepository
{
    Task AddAsync(Report report);
    IQueryable<Report> GetReportsQueryable();
}