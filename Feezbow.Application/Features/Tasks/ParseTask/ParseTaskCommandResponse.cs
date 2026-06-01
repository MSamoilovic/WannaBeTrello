using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Tasks.ParseTask;

public record ParseTaskCommandResponse(ParseTaskProposalDto Proposal);

public record ParseTaskProposalDto(
    string Title,
    string Description,
    TaskPriority Priority,
    DateOnly? DueDate,
    long? AssigneeUserId,
    string? AssigneeName
);
