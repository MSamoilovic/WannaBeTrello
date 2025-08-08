using MediatR;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public class CreateProjectCommand: IRequest<long>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
}