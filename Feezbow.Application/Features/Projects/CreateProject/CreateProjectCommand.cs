using MediatR;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public record CreateProjectCommand(string? Name, string? Description) : IRequest<CreateProjectCommandResponse>;
