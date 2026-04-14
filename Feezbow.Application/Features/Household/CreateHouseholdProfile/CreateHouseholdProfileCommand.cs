using MediatR;

namespace Feezbow.Application.Features.Household.CreateHouseholdProfile;

public record CreateHouseholdProfileCommand(
    long ProjectId,
    string? Address,
    string? City,
    string? Timezone,
    DayOfWeek? ShoppingDay) : IRequest<CreateHouseholdProfileCommandResponse>;
