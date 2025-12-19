using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    IMediaService mediaService,
    IPhotoRepository photoRepository
) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = mapper.Map<Category>(command.CreateCategoryDto);
        var dto = command.CreateCategoryDto;

        // check if creating a root category
        if (dto.ParentCategoryId == null)
        {
            // check if root category with this name already exists
            var rootExists = await categoryRepository.RootCategoryExistsByName(dto.Name);
            if (rootExists)
                throw new BadRequestException("A root category with that name already exists");
            if (command.Image == null)
            {
                throw new BadRequestException("Root category must have an image");
            }
        }
        else
        {
            if (command.Image != null)
            {
                throw new BadRequestException("Non root category cannot have an image");
            }
            var parentCategory = await categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parentCategory == null)
                throw new NotFoundException("Parent category not found");

            // if parent has a root reference set the new category root to the same id, if not the parent is the root
            var rootCategoryId = parentCategory.RootCategoryId ?? parentCategory.Id;

            var nameExistsInTree = await categoryRepository.CategoryExistsByNameInTree(dto.Name, rootCategoryId);
            if (nameExistsInTree)
                throw new BadRequestException(
                    $"A category with name '{dto.Name}' already exists in this category tree");

            // Set the root category ID for the new category
            category.RootCategoryId = rootCategoryId;
        }

        string savedPhotoPublicId = string.Empty;
        await unitOfWork.BeginTransactionAsync();
        try
        {
            if (command.Image != null)
            {
                var uploadResult = await mediaService.UploadPhotoAsync(command.Image);
                if (uploadResult.Error != null)
                {
                    await mediaService.DeleteImageAsync(uploadResult.PublicId);
                    throw new CloudinaryException(uploadResult.Error.Message);
                }
            
                savedPhotoPublicId = uploadResult.PublicId;
            
                var photo = new Photo
                {
                    Url = uploadResult.SecureUrl.AbsoluteUri,
                    PublicId = uploadResult.PublicId,
                    FileName = command.Image.FileName
                };
                await photoRepository.AddPhotoAsync(photo);

                category.Photo = photo;   
            }
            await categoryRepository.CreateCategory(category);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (savedPhotoPublicId != string.Empty)
            {
                await mediaService.DeleteImageAsync(savedPhotoPublicId);
            }
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
       

        // remove cache after adding new entity for getting fresh data 
        if (category.ParentCategoryId != null && category.RootCategoryId != null)
        {
            await cache.RemoveAsync($"{CacheKeys.SUB_CATEGORIES}{category.ParentCategoryId.Value}");
            await cache.RemoveAsync($"{CacheKeys.CATEGORY_TREE}{category.RootCategoryId.Value}");
        }
        else
            await cache.RemoveAsync(CacheKeys.MAIN_CATEGORIES);

        await cache.RemoveAsync(CacheKeys.ALL_CATEGORIES);

        return category.Id;
    }
}