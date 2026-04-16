using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IMapper _mapper;

    public PollService(IPollRepository pollRepository, IMapper mapper)
    {
        _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Guid> CreatePollAsync(CreatePollDto createPollDto)
    {
        if (createPollDto == null)
            throw new ArgumentNullException(nameof(createPollDto));

        if (string.IsNullOrWhiteSpace(createPollDto.Title))
            throw new ArgumentException("Poll title cannot be null or empty", nameof(createPollDto.Title));

        var poll = _mapper.Map<Poll>(createPollDto);
        poll.PollId = Guid.NewGuid();
        poll.PublishedAt = DateTime.UtcNow;
        poll.IsActive = true;

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
}

