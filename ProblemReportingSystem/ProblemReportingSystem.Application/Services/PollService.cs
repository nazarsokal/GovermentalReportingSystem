using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using System.Linq;

namespace ProblemReportingSystem.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public PollService(IPollRepository pollRepository, IUserRepository userRepository, IMapper mapper)
    {
        _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Guid> CreatePollAsync(CreatePollDto createPollDto)
    {
        if (createPollDto == null)
            throw new ArgumentNullException(nameof(createPollDto));

        if (string.IsNullOrWhiteSpace(createPollDto.Title))
            throw new ArgumentException("Poll title cannot be null or empty", nameof(createPollDto.Title));

        var poll = _mapper.Map<Poll>(createPollDto);
        var cleanOptions = createPollDto.Options
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Select(option => option.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleanOptions.Count < 2)
            throw new ArgumentException("Poll must contain at least 2 unique options", nameof(createPollDto.Options));

        poll.PollId = Guid.NewGuid();
        poll.PublishedAt = DateTime.UtcNow;
        poll.IsActive = true;
        poll.PollOptions = cleanOptions
            .Select(option => new PollOption
            {
                OptionId = Guid.NewGuid(),
                PollId = poll.PollId,
                OptionText = option
            })
            .ToList();

        await _pollRepository.CreateAsync(poll);
        return poll.PollId;
    }

    public async Task<IEnumerable<PollDetailsDto>> GetCouncilPollsAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var polls = await _pollRepository.GetCouncilPollsAsync(councilId);
        return _mapper.Map<IEnumerable<PollDetailsDto>>(polls);
    }

    public async Task<PollDetailsDto?> GetPollByIdAsync(Guid pollId)
    {
        if (pollId == Guid.Empty)
            throw new ArgumentException("Poll ID cannot be empty", nameof(pollId));

        var poll = await _pollRepository.GetPollWithDetailsAsync(pollId);
        return poll == null ? null : _mapper.Map<PollDetailsDto>(poll);
    }

    public async Task<bool> ClosePollAsync(Guid pollId)
    {
        if (pollId == Guid.Empty)
            throw new ArgumentException("Poll ID cannot be empty", nameof(pollId));

        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null)
            return false;

        poll.IsActive = false;
        await _pollRepository.UpdateAsync(poll);
        return true;
    }

    public async Task<PollDetailsDto?> GetPollWithResultsAsync(Guid pollId)
    {
        if (pollId == Guid.Empty)
            throw new ArgumentException("Poll ID cannot be empty", nameof(pollId));

        var poll = await _pollRepository.GetPollWithVotesAsync(pollId);
        return poll == null ? null : _mapper.Map<PollDetailsDto>(poll);
    }

    public async Task<IEnumerable<PollDetailsDto>> GetActivePollsForDistrictAsync(string district, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(district))
            return Enumerable.Empty<PollDetailsDto>();

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var normalizedRequestedDistrict = NormalizeDistrict(district);
        var polls = await _pollRepository.GetActivePollsByDistrictAsync(district);
        var filteredPolls = polls.Where(p =>
        {
            var councilDistrict = p.Council?.Address?.District;
            if (string.IsNullOrWhiteSpace(councilDistrict))
                return false;

            var normalizedCouncilDistrict = NormalizeDistrict(councilDistrict);
            return normalizedRequestedDistrict == normalizedCouncilDistrict
                   || normalizedRequestedDistrict.Contains(normalizedCouncilDistrict)
                   || normalizedCouncilDistrict.Contains(normalizedRequestedDistrict);
        }).ToList();
        var pollDtos = _mapper.Map<List<PollDetailsDto>>(filteredPolls);

        foreach (var poll in pollDtos)
        {
            poll.TotalVotes = poll.PollOptions.Sum(x => x.VoteCount);
            poll.UserHasVoted = await _pollRepository.UserHasVotedInPollAsync(poll.PollId, userId);
        }

        return pollDtos;
    }

    public async Task<bool> VoteAsync(Guid pollId, Guid optionId, Guid userId)
    {
        if (pollId == Guid.Empty || optionId == Guid.Empty || userId == Guid.Empty)
            return false;

        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user?.Address?.District == null)
            return false;

        var poll = await _pollRepository.GetPollWithDetailsAsync(pollId);
        if (poll?.IsActive != true)
            return false;

        var councilDistrict = poll.Council?.Address?.District;
        if (string.IsNullOrWhiteSpace(councilDistrict) ||
            !string.Equals(councilDistrict.Trim(), user.Address.District.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!await _pollRepository.OptionBelongsToPollAsync(pollId, optionId))
            return false;

        if (await _pollRepository.UserHasVotedInPollAsync(pollId, userId))
            return false;

        await _pollRepository.CreateVoteAsync(new PollVote
        {
            VoteId = Guid.NewGuid(),
            UserId = userId,
            OptionId = optionId,
            VotedAt = DateTime.UtcNow
        });

        return true;
    }

    private static string NormalizeDistrict(string district)
    {
        if (string.IsNullOrWhiteSpace(district))
            return string.Empty;

        return district
            .Trim()
            .ToLowerInvariant()
            .Replace("район", string.Empty)
            .Replace("district", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);
    }
}

