using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.API.Contracts.Request;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for problem management operations.
/// Provides endpoints for creating, updating, and retrieving problems reported by users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProblemController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProblemController> _logger;

    public ProblemController(IProblemService problemService, IMapper mapper, ILogger<ProblemController> logger)
    {
        _problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> CreateProblem([FromForm] CreateProblemRequest request, CancellationToken cancellationToken = default)
{
    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Invalid request model state for CreateProblem");
        return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
    }

    // 1. Мапимо базові текстові поля (назва, опис, координати) з Request у DTO
    var createProblemDto = _mapper.Map<CreateProblemDto>(request);
    
    // Ініціалізуємо колекцію фотографій, якщо вона ще не створена
    createProblemDto.Photos = new List<CreateProblemPhotoDto>();

    if (request.Photos != null && request.Photos.Any())
    {
        foreach (var file in request.Photos)
        {
            if (file.Length > 0)
            {
                using var memoryStream = new MemoryStream();

                await file.CopyToAsync(memoryStream, cancellationToken);
                
                createProblemDto.Photos.Add(new CreateProblemPhotoDto
                {
                    ImageData = memoryStream.ToArray(),
                    ContentType = file.ContentType 
                });
            }
        }
    }
    
    // 3. Передаємо DTO з уже підготовленими байтами у сервіс
    var problemId = await _problemService.CreateProblem(createProblemDto);

    _logger.LogInformation($"Problem created successfully with ID: {problemId}");
    return CreatedAtAction(nameof(CreateProblem), new { id = problemId }, new { id = problemId });
}
}