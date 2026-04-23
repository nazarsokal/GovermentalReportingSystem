using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemCategoriesController : ControllerBase
{
    private readonly IProblemCategoryService _categoryService;

    public ProblemCategoriesController(IProblemCategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet("problem-categories")]
    public async Task<IActionResult> GetAllProblemCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}