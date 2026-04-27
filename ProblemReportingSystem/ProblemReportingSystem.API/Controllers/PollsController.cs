using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.API.Contracts.Request;
using ProblemReportingSystem.API.Contracts.Response;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using System.Linq;
using System.Security.Claims;

namespace ProblemReportingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly IUserRepository _userRepository;

    public PollsController(IPollService pollService, IUserRepository userRepository)
    {
        _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet("district-active")]
    [ProducesResponseType(typeof(List<PollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveDistrictPolls([FromQuery] string? district = null)
    {
        var userId = GetCurrentUserId();
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        var userDistrict = user?.Address?.District;
        var requestedDistrict = string.IsNullOrWhiteSpace(district) ? null : district.Trim();

        var targetDistrict = !string.IsNullOrWhiteSpace(userDistrict) ? userDistrict : requestedDistrict;

        if (string.IsNullOrWhiteSpace(targetDistrict))
        {
            return Ok(new List<PollResponse>());
        }

        var polls = await _pollService.GetActivePollsForDistrictAsync(targetDistrict, userId);
        var response = polls.Select(p => new PollResponse
        {
            PollId = p.PollId,
            Title = p.Title,
            Description = p.Description,
            PublishedAt = p.PublishedAt,
            IsActive = p.IsActive,
            UserHasVoted = p.UserHasVoted,
            TotalVotes = p.TotalVotes,
            Options = p.PollOptions.Select(opt => new PollOptionResponse
            {
                OptionId = opt.OptionId,
                OptionText = opt.OptionText,
                VoteCount = opt.VoteCount,
                VotePercentage = p.TotalVotes > 0
                    ? Math.Round((decimal)opt.VoteCount / p.TotalVotes * 100m, 2)
                    : 0m
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpPost("{pollId:guid}/vote")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Vote(Guid pollId, [FromBody] VotePollRequest request)
    {
        if (pollId == Guid.Empty || request == null || request.OptionId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "Invalid poll vote request" });
        }

        var userId = GetCurrentUserId();
        var voted = await _pollService.VoteAsync(pollId, request.OptionId, userId);
        if (!voted)
        {
            return BadRequest(new { success = false, message = "Unable to submit vote. You may have already voted or poll is unavailable." });
        }

        return Ok(new { success = true, message = "Vote submitted successfully" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }

        return userId;
    }
}
