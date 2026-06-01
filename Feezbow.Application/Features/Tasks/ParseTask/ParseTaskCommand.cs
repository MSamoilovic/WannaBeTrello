using MediatR;

namespace Feezbow.Application.Features.Tasks.ParseTask;

public record ParseTaskCommand(long ProjectId, string FreeText)
    : IRequest<ParseTaskCommandResponse>;
