using Microsoft.Extensions.Logging;
using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

/// <summary>
/// Implementation of problem category service
/// </summary>
public class ProblemCategoryService : IProblemCategoryService
{
    private readonly IProblemCategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProblemCategoryService> _logger;

    public ProblemCategoryService(
        IProblemCategoryRepository repository,
        IMapper mapper,
        ILogger<ProblemCategoryService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    public async Task<ProblemCategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

            _logger.LogInformation($"Retrieving category with ID: {categoryId}");
            var category = await _repository.GetByIdAsync(categoryId);
            
            if (category == null)
            {
                _logger.LogWarning($"Category with ID {categoryId} not found");
                return null;
            }

            return _mapper.Map<ProblemCategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving category {categoryId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<IEnumerable<ProblemCategoryDto>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all categories");
            var categories = _repository.GetAll().ToList();
            return _mapper.Map<IEnumerable<ProblemCategoryDto>>(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all categories: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    public async Task<ProblemCategoryDto> CreateCategoryAsync(CreateProblemCategoryDto createCategoryDto)
    {
        try
        {
            if (createCategoryDto == null)
                throw new ArgumentNullException(nameof(createCategoryDto));

            if (string.IsNullOrWhiteSpace(createCategoryDto.Name))
                throw new ArgumentException("Category name cannot be empty", nameof(createCategoryDto.Name));

            _logger.LogInformation($"Creating new category: {createCategoryDto.Name}");
            
            var category = _mapper.Map<ProblemCategory>(createCategoryDto);
            category.CategoryId = Guid.NewGuid();

            var createdCategory = await _repository.CreateAsync(category);
            return _mapper.Map<ProblemCategoryDto>(createdCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating category: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    public async Task<ProblemCategoryDto?> UpdateCategoryAsync(UpdateProblemCategoryDto updateCategoryDto)
    {
        try
        {
            if (updateCategoryDto == null)
                throw new ArgumentNullException(nameof(updateCategoryDto));

            if (updateCategoryDto.CategoryId == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty", nameof(updateCategoryDto.CategoryId));

            if (string.IsNullOrWhiteSpace(updateCategoryDto.Name))
                throw new ArgumentException("Category name cannot be empty", nameof(updateCategoryDto.Name));

            _logger.LogInformation($"Updating category: {updateCategoryDto.CategoryId}");
            
            var category = await _repository.GetByIdAsync(updateCategoryDto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning($"Category {updateCategoryDto.CategoryId} not found");
                return null;
            }

            _mapper.Map(updateCategoryDto, category);
            var updatedCategory = await _repository.UpdateAsync(category);
            return _mapper.Map<ProblemCategoryDto>(updatedCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating category: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        try
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

            _logger.LogInformation($"Deleting category: {categoryId}");
            return await _repository.DeleteAsync(categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting category: {ex.Message}");
            throw;
        }
    }
}
