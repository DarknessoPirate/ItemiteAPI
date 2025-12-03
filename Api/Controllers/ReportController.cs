using Application.Features.Reports.CreateReport;
using Domain.DTOs.File;
using Domain.DTOs.Reports;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IMediator mediator, IRequestContextService requestContextService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("CreateReportPolicy")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateReport(
        [FromForm] CreateReportRequest dto,
        [FromForm] List<IFormFile>? images)
    {
        var fileWrappers = new List<FileWrapper>();
        foreach (var image in images ?? new List<IFormFile>())
        {
            fileWrappers.Add(new FileWrapper(image.FileName, image.Length, image.ContentType, image.OpenReadStream()));
        }

        var command = new CreateReportCommand
        {
            UserId = requestContextService.GetUserId(),
            ReportDto = dto,
            Images = fileWrappers.Count == 0 ? null : fileWrappers,
        };
        
        var reportId = await mediator.Send(command);
        return Ok(new {reportId});
    }
}