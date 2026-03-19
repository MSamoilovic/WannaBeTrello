using MediatR;

namespace Feezbow.Application.Features.Projects.CreateProject;

public record CreateProjectCommand(string? Name, string? Description) : IRequest<CreateProjectCommandResponse>;
