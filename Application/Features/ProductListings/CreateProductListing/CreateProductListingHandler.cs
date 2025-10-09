using AutoMapper;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.ProductListings.CreateProductListing;

public class CreateProductListingHandler(
        ICurrentUserService currentUser,
        IListingRepository<ProductListing> productListingRepository,
        ICategoryRepository categoryRepository,
        UserManager<User> userManager,
        IMapper mapper,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<CreateProductListingCommand, int>
{
    public async Task<int> Handle(CreateProductListingCommand request, CancellationToken cancellationToken)
    {
        var productListing = mapper.Map<ProductListing>(request.ProductListingDto);
        productListing.OwnerId = currentUser.GetId();
        
        var category = await categoryRepository.GetByIdAsync(request.ProductListingDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (await categoryRepository.IsParentCategory(request.ProductListingDto.CategoryId))
        {
            throw new BadRequestException("You can't assign parent category to product listing");
        }

        var directParents = await categoryRepository.GetAllParentsRelatedToCategory(category);
        directParents.Add(category);
        
        productListing.Categories = directParents;

        if (request.ProductListingDto.Location == null)
        {
            var user = await userManager.FindByEmailAsync(currentUser.GetEmail());
            productListing.Location = user.Location;
        }
        
        // TODO: handle images upload (CloudinaryService needed)
        
        await productListingRepository.CreateListingAsync(productListing);
        await unitOfWork.SaveChangesAsync();
        
        return productListing.Id;
    }
}