using AutoMapper;
using Domain.DTOs.Category;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Categories.CreateCategory;

public class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper
    ) : IRequestHandler<CreateCategoryCommand>
{
    public async Task Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = mapper.Map<Category>(command.CreateCategoryDto);
        await categoryRepository.CreateCategory(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }            
}            