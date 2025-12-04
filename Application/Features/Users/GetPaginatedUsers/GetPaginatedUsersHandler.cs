using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.User;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.GetPaginatedUsers;

public class GetPaginatedUsersHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetPaginatedUsersQuery, PageResponse<UserResponse>>
{
    public async Task<PageResponse<UserResponse>> Handle(GetPaginatedUsersQuery request, CancellationToken cancellationToken)
    {
        var usersQuery = userRepository.GetUsersQueryable();
        
        int totalItems = await usersQuery.CountAsync(cancellationToken);

        if (!string.IsNullOrEmpty(request.Query.Search))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(request.Query.Search) || u.UserName.Contains(request.Query.Search));
        }
        
        usersQuery = usersQuery
            .OrderBy(u => u.UserName)
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize);
        
        var mappedUsers = mapper.Map<List<UserResponse>>(await usersQuery.ToListAsync());
        
        return new PageResponse<UserResponse>(mappedUsers, totalItems, request.Query.PageSize, request.Query.PageNumber);
    }
}