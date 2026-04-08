using System.Net;
using System.Net.Http.Json;
using Feezbow.Application.Features.Boards.UpdateBoard;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Boards;

[Collection("Integration")]
public class BoardControllerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateBoard_WithValidData_ReturnsCreated()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync("/api/v1/boards",
            new { ProjectId = projectId, Name = "Sprint 1" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBoard_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/boards",
            new { ProjectId = 1, Name = "Board" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBoardById_ExistingBoard_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        var response = await Client.GetAsync($"/api/v1/boards/{boardId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBoardById_NonExistentId_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/v1/boards/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_WithValidData_ReturnsNoContent()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        var command = new UpdateBoardCommand { Id = boardId, Name = "Updated Board", Description = "Updated" };
        var response = await Client.PutAsJsonAsync($"/api/v1/boards/{boardId}", command);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoard_IdMismatch_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        var command = new UpdateBoardCommand { Id = boardId + 1, Name = "Board" };
        var response = await Client.PutAsJsonAsync($"/api/v1/boards/{boardId}", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveBoard_ExistingBoard_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        var response = await Client.PostAsync($"/api/v1/boards/{boardId}/archive", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RestoreBoard_AfterArchive_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        await Client.PostAsync($"/api/v1/boards/{boardId}/archive", null);
        var response = await Client.PostAsync($"/api/v1/boards/{boardId}/restore", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetColumnsByBoardId_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var (_, boardId) = await CreateProjectAndBoardAsync();

        var response = await Client.GetAsync($"/api/v1/boards/{boardId}/columns");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- helpers ---

    private async Task<long> CreateProjectAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = "Test Project", Description = (string?)null });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateProject response was null.");
        return result.Result.Value;
    }

    private async Task<(long projectId, long boardId)> CreateProjectAndBoardAsync()
    {
        var projectId = await CreateProjectAsync();
        var boardResponse = await Client.PostAsJsonAsync("/api/v1/boards",
            new { ProjectId = projectId, Name = "Test Board" });
        boardResponse.EnsureSuccessStatusCode();
        var boardResult = await boardResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateBoard response was null.");
        return (projectId, boardResult.Result.Value);
    }
}
