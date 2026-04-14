using MediatR;

namespace Feezbow.Application.Features.Household.GetHouseholdProfile;

public record GetHouseholdProfileQuery(long ProjectId) : IRequest<GetHouseholdProfileQueryResponse>;
