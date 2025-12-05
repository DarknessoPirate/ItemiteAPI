using Domain.DTOs.Pagination;
using Domain.DTOs.User;
using MediatR;

namespace Application.Features.Users.GetPaginatedUsers;

public class GetPaginatedUsersQuery : IRequest<PageResponse<UserResponse>>
{
    public PaginateUsersQuery Query { get; set; }
}