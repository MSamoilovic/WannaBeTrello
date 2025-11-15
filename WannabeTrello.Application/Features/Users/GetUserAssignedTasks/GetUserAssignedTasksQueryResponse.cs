using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Users.GetUserAssignedTasks;

public class GetUserAssignedTasksQueryResponse
{
    public IReadOnlyList<UserAssignedTaskDto> Tasks { get; init; } = [];

    public static GetUserAssignedTasksQueryResponse FromEntities(IReadOnlyList<BoardTask> tasks)
    {
        return new GetUserAssignedTasksQueryResponse
        {
            Tasks = tasks.Select(UserAssignedTaskDto.FromEntity).ToList()
        };
    }
}

public class UserAssignedTaskDto
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
   

    public static UserAssignedTaskDto FromEntity(BoardTask task)
    {
        return new UserAssignedTaskDto
        {
            Id = task.Id,
            Title = task.Title
        };
    }
}

