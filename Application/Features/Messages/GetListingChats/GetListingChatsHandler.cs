using Domain.DTOs.Messages;
using Domain.DTOs.Pagination;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Messages.GetListingChats;

public class GetListingChatsHandler(
    UserManager<User> userManager,
    IMessageRepository messageRepository,
    IListingRepository<ListingBase> listingRepository,
    
    ) : IRequestHandler<GetListingChatsCommand, PageResponse<ChatInfoResponse>>
{
    public async Task<PageResponse<ChatInfoResponse>> Handle(GetListingChatsCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("User not found");
        
        var listing = await listingRepository.GetListingByIdAsync()
    }
}