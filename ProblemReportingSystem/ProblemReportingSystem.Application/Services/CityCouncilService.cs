using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProblemReportingSystem.Application.Services;

public class CityCouncilService : ICityCouncilService
{
    private readonly ICityCouncilRepository _councilRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICouncilEmployeeRepository _councilEmployeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeolocateService _geolocateService;
    private readonly IMapper _mapper;
    private readonly ILogger<CityCouncilService> _logger;

    public CityCouncilService(
        ICityCouncilRepository councilRepository,
        IAddressRepository addressRepository,
        ICouncilEmployeeRepository councilEmployeeRepository,
        IUserRepository userRepository,
        IGeolocateService geolocateService,
        IMapper mapper,
        ILogger<CityCouncilService> logger)
    {
        _councilRepository = councilRepository ?? throw new ArgumentNullException(nameof(councilRepository));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _councilEmployeeRepository = councilEmployeeRepository ?? throw new ArgumentNullException(nameof(councilEmployeeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _geolocateService = geolocateService ?? throw new ArgumentNullException(nameof(geolocateService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> CreateCityCouncilAsync(CreateCityCouncilDto createCityCouncilDto)
    {
        if (createCityCouncilDto == null)
            throw new ArgumentNullException(nameof(createCityCouncilDto));

        if (string.IsNullOrWhiteSpace(createCityCouncilDto.Name))
            throw new ArgumentException("Council name cannot be null or empty", nameof(createCityCouncilDto.Name));

        if (createCityCouncilDto.HeadUserId == Guid.Empty)
            throw new ArgumentException("Head user ID cannot be empty", nameof(createCityCouncilDto.HeadUserId));

        // Check if council already exists
        var existingCouncil = await _councilRepository.GetCouncilByNameAsync(createCityCouncilDto.Name);
        if (existingCouncil != null)
            throw new InvalidOperationException($"Council with name '{createCityCouncilDto.Name}' already exists");

        // Verify head user exists
        var headUser = await _userRepository.GetByIdWithRolesAsync(createCityCouncilDto.HeadUserId);
        if (headUser == null)
            throw new InvalidOperationException($"User with ID '{createCityCouncilDto.HeadUserId}' not found");

        try
        {
            var addressFromCoordinates = await _geolocateService.GetAddressFromCoordinatesAsync(createCityCouncilDto.Latitude, createCityCouncilDto.Longitude);
            // Get coordinates from address using geolocation service
            // var addressResult = await GetCoordinatesFromAddressAsync(createCityCouncilDto);

            // Create address entity
            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                City = addressFromCoordinates.City,
                Street = addressFromCoordinates.Street,
                BuildingNumber = addressFromCoordinates.BuildingNumber,
                District = addressFromCoordinates.District,
                Oblast = addressFromCoordinates.Oblast,
                Country = addressFromCoordinates.Country ?? "Ukraine",
                Postcode = addressFromCoordinates.Postcode,
                Latitude = createCityCouncilDto.Latitude,
                Longitude = createCityCouncilDto.Longitude
            };

            await _addressRepository.CreateAsync(address);
            _logger.LogInformation($"Address created with ID: {address.AddressId}");

            // Create city council
            var council = new CityCouncil
            {
                CouncilId = Guid.NewGuid(),
                Name = createCityCouncilDto.Name,
                ContactEmail = createCityCouncilDto.ContactEmail,
                AddressId = address.AddressId
            };

            await _councilRepository.CreateAsync(council);
            _logger.LogInformation($"City council created successfully with ID: {council.CouncilId}, Name: {council.Name}");

            // Create council employee record for the head user
            var councilEmployee = new CouncilEmployee
            {
                EmployeeId = Guid.NewGuid(),
                CouncilId = council.CouncilId,
                UserId = createCityCouncilDto.HeadUserId,
                Position = "Head"
            };

            await _councilEmployeeRepository.CreateAsync(councilEmployee);
            _logger.LogInformation($"Council head assigned with ID: {councilEmployee.EmployeeId}, User ID: {createCityCouncilDto.HeadUserId}");

            return council.CouncilId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating city council: {ex.Message}", ex);
            throw;
        }
    }
    
    public async Task<IEnumerable<CityCouncilDto>> GetAllCityCouncilsAsync()
    {
        var councils = await _councilRepository.GetAllCouncilsAsync();
        return _mapper.Map<IEnumerable<CityCouncilDto>>(councils);
    }

    public async Task<IEnumerable<CityCouncilWithAddressDto>> GetAllCityCouncilsWithAddressAsync()
    {
        var councils = await _councilRepository.GetAllCouncilsAsync();
        return councils.Select(c => new CityCouncilWithAddressDto
        {
            CouncilId = c.CouncilId,
            Name = c.Name,
            ContactEmail = c.ContactEmail,
            City = c.Address?.City,
            District = c.Address?.District,
            Oblast = c.Address?.Oblast
        });
    }

    public async Task<CityCouncilDetailsDto?> GetCityCouncilByIdAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var council = await _councilRepository.GetCouncilWithDetailsAsync(councilId);
        return council == null ? null : _mapper.Map<CityCouncilDetailsDto>(council);
    }
    
    public async Task<IEnumerable<CityCouncilDto>> LoadCityCouncilsFromCsvAsync(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
            throw new ArgumentException("CSV file cannot be null or empty", nameof(csvFile));

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must be a CSV file", nameof(csvFile));

        var createdCouncils = new List<CityCouncilDto>();

        try
        {
            using var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8);
            string? headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
                throw new InvalidOperationException("CSV file is empty or has no headers");

            // Parse header
            var headers = ParseCsvLine(headerLine);
            var nameIndex = headers.FindIndex(h => h.Equals("Name", StringComparison.OrdinalIgnoreCase));
            var emailIndex = headers.FindIndex(h => h.Equals("ContactEmail", StringComparison.OrdinalIgnoreCase));

            if (nameIndex == -1)
                throw new InvalidOperationException("CSV file must contain a 'Name' column");

            // Parse data rows
            string? line;
            int lineNumber = 2;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var values = ParseCsvLine(line);

                    if (values.Count <= nameIndex)
                    {
                        _logger.LogWarning($"Row {lineNumber} has fewer columns than expected, skipping");
                        lineNumber++;
                        continue;
                    }

                    var name = values[nameIndex]?.Trim();
                    var contactEmail = emailIndex >= 0 && emailIndex < values.Count ? values[emailIndex]?.Trim() : null;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _logger.LogWarning($"Row {lineNumber} has empty name, skipping");
                        lineNumber++;
                        continue;
                    }

                    // Check if council already exists
                    var existingCouncil = await _councilRepository.GetCouncilByNameAsync(name);
                    if (existingCouncil != null)
                    {
                        _logger.LogWarning($"Council '{name}' already exists, skipping");
                        lineNumber++;
                        continue;
                    }

                    var createDto = new CreateCityCouncilDto
                    {
                        Name = name,
                        ContactEmail = contactEmail
                    };

                    var councilId = await CreateCityCouncilAsync(createDto);
                    createdCouncils.Add(new CityCouncilDto
                    {
                        CouncilId = councilId,
                        Name = name,
                        ContactEmail = contactEmail
                    });

                    _logger.LogInformation($"Council '{name}' loaded from CSV at line {lineNumber}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing row {lineNumber}: {ex.Message}");
                    throw new InvalidOperationException($"Error processing row {lineNumber}: {ex.Message}", ex);
                }

                lineNumber++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading CSV file: {ex.Message}");
            throw new InvalidOperationException($"Error reading CSV file: {ex.Message}", ex);
        }

        _logger.LogInformation($"Successfully loaded {createdCouncils.Count} city councils from CSV");
        return createdCouncils;
    }

    /// <summary>
    /// Parses a CSV line handling quoted values and escaped quotes.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    // Toggle quote state
                    insideQuotes = !insideQuotes;
                }
            }
            else if (ch == ',' && !insideQuotes)
            {
                // End of field
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(ch);
            }
        }

        // Add the last field
        values.Add(currentValue.ToString().Trim());

        return values;
    }
}
