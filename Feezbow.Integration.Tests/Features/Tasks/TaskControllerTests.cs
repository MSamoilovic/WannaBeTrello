using System.Net;
using System.Net.Http.Json;
using Feezbow.Application.Features.Tasks.AddCommentToTask;
using Feezbow.Application.Features.Tasks.AssignTaskToUser;
using Feezbow.Application.Features.Tasks.CreateTask;
using Feezbow.Application.Features.Tasks.MoveTask;
using Feezbow.Application.Features.Tasks.UpdateTask;
using Feezbow.Domain.Enums;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Tasks;

[Collection("Integration")]
public class TaskControllerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateTask_WithValidData_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (columnId, _) = await CreateColumnAndBoardAsync();

        var response = await Client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskCommand
        {
            ColumnId = columnId,
            Title = "Fix bug",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(3),
            Position = 0
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskCommand
        {
            ColumnId = 1,
            Title = "Task",
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(1),
            Position = 0
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var response = await Client.GetAsync($"/api/v1/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskById_NonExistentId_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/v1/tasks/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByBoardId_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateColumnAndBoardAsync();

        var response = await Client.GetAsync($"/api/v1/tasks/board/{boardId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "Updated title",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(5)
        };
        var response = await Client.PutAsJsonAsync($"/api/v1/tasks/{taskId}", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_IdMismatch_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var command = new UpdateTaskCommand
        {
            TaskId = taskId + 1,
            Title = "Title",
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(1)
        };
        var response = await Client.PutAsJsonAsync($"/api/v1/tasks/{taskId}", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveTask_ToAnotherColumn_ReturnsNoContent()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (columnId, boardId) = await CreateColumnAndBoardAsync();
        var taskId = await CreateTaskInColumnAsync(columnId);

        // Kreiraj drugu kolonu na istom boardu
        var col2Response = await Client.PostAsJsonAsync("/api/v1/column",
            new { BoardId = boardId, Name = "Done", Order = 2 });
        col2Response.EnsureSuccessStatusCode();
        var col2 = await col2Response.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException();

        var response = await Client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/move",
            new MoveTaskCommand(taskId, col2.Result.Value));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task MoveTask_IdMismatch_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (columnId, _) = await CreateColumnAndBoardAsync();
        var taskId = await CreateTaskInColumnAsync(columnId);

        var response = await Client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/move",
            new MoveTaskCommand(taskId + 1, columnId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveTask_ExistingTask_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var response = await Client.PutAsync($"/api/v1/tasks/{taskId}/archive", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTask_AfterArchive_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        await Client.PutAsync($"/api/v1/tasks/{taskId}/archive", null);
        var response = await Client.PutAsync($"/api/v1/tasks/{taskId}/restore", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_ToExistingTask_ReturnsCreated()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/tasks/{taskId}/comments",
            new AddCommentToTaskCommand { TaskId = taskId, Content = "Great progress!" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetCommentsByTaskId_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var taskId = await CreateTaskAsync();

        var response = await Client.GetAsync($"/api/v1/tasks/{taskId}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignTask_ToValidUser_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var userId = await GetCurrentUserIdAsync();
        var taskId = await CreateTaskAsync();

        var response = await Client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/assign",
            new AssignTaskToUserCommand(taskId, userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- helpers ---

    private async Task<(long columnId, long boardId)> CreateColumnAndBoardAsync()
    {
        var projectResponse = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = "Test Project", Description = (string?)null });
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException();

        var boardResponse = await Client.PostAsJsonAsync("/api/v1/boards",
            new { ProjectId = project.Result.Value, Name = "Test Board" });
        boardResponse.EnsureSuccessStatusCode();
        var board = await boardResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException();

        var colResponse = await Client.PostAsJsonAsync("/api/v1/column",
            new { BoardId = board.Result.Value, Name = "To Do", Order = 1 });
        colResponse.EnsureSuccessStatusCode();
        var column = await colResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException();

        return (column.Result.Value, board.Result.Value);
    }

    private async Task<long> CreateTaskInColumnAsync(long columnId)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskCommand
        {
            ColumnId = columnId,
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(7),
            Position = 0
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateTask response was null.");
        return result.Result.Value;
    }

    private async Task<long> CreateTaskAsync()
    {
        var (columnId, _) = await CreateColumnAndBoardAsync();
        return await CreateTaskInColumnAsync(columnId);
    }
}
