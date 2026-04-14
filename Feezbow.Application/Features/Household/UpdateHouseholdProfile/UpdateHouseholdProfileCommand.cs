using MediatR;

namespace Feezbow.Application.Features.Household.UpdateHouseholdProfile;

public record UpdateHouseholdProfileCommand(
    long ProjectId,
    string? Address,
    string? City,
    string? Timezone,
    DayOfWeek? ShoppingDay) : IRequest<UpdateHouseholdProfileCommandResponse>;
