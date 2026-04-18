using MediatR;

namespace Feezbow.Application.Features.Household.GetHouseholdMembers;

public record GetHouseholdMembersQuery(long ProjectId) : IRequest<GetHouseholdMembersQueryResponse>;
