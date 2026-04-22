using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

/// <summary>
/// Service interface for problem category operations
/// </summary>
public interface IProblemCategoryService
{
    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <returns>The category DTO if found, null otherwise</returns>
    Task<ProblemCategoryDto?> GetCategoryByIdAsync(Guid categoryId);

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of all categories</returns>
    Task<IEnumerable<ProblemCategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="createCategoryDto">The category creation DTO</param>
    /// <returns>The created category</returns>
    Task<ProblemCategoryDto> CreateCategoryAsync(CreateProblemCategoryDto createCategoryDto);

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="updateCategoryDto">The category update DTO</param>
    /// <returns>The updated category if found, null otherwise</returns>
    Task<ProblemCategoryDto?> UpdateCategoryAsync(UpdateProblemCategoryDto updateCategoryDto);

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="categoryId">The category ID to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteCategoryAsync(Guid categoryId);
}

