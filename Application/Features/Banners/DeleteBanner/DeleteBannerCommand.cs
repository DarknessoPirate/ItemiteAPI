using MediatR;

namespace Application.Features.Banners.DeleteBanner;

public class DeleteBannerCommand : IRequest
{
    public int UserId { get; set; }
    public int BannerId { get; set; }
    public bool ForceDelete { get; set; }
}