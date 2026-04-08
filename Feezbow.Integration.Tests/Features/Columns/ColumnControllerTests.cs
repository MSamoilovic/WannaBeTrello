using System.Net;
using System.Net.Http.Json;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Columns;

[Collection("Integration")]
public class ColumnControllerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateColumn_WithValidData_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var boardId = await CreateBoardAsync();

        var response = await Client.PostAsJsonAsync("/api/v1/column",
            new { BoardId = boardId, Name = "To Do", Order = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateColumn_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/column",
            new { BoardId = 1, Name = "To Do", Order = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetColumnById_ExistingColumn_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var columnId = await CreateColumnAsync();

        var response = await Client.GetAsync($"/api/v1/column/{columnId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetColumnById_NonExistentId_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/v1/column/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateColumn_WithValidData_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var columnId = await CreateColumnAsync();

        var response = await Client.PutAsJsonAsync($"/api/v1/column/{columnId}",
            new { ColumnId = columnId, NewName = "In Progress", WipLimit = (int?)null });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateColumn_IdMismatch_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var columnId = await CreateColumnAsync();

        var response = await Client.PutAsJsonAsync($"/api/v1/column/{columnId}",
            new { ColumnId = columnId + 1, NewName = "Name", WipLimit = (int?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteColumn_ExistingColumn_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var columnId = await CreateColumnAsync();

        var response = await Client.DeleteAsync($"/api/v1/column/{columnId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteColumn_NonExistentId_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.DeleteAsync("/api/v1/column/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- helpers ---

    private async Task<long> CreateBoardAsync()
    {
        var projectResponse = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = "Test Project", Description = (string?)null });
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateProject response was null.");

        var boardResponse = await Client.PostAsJsonAsync("/api/v1/boards",
            new { ProjectId = project.Result.Value, Name = "Test Board" });
        boardResponse.EnsureSuccessStatusCode();
        var board = await boardResponse.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateBoard response was null.");

        return board.Result.Value;
    }

    private async Task<long> CreateColumnAsync(string name = "To Do", int order = 1)
    {
        var boardId = await CreateBoardAsync();
        var response = await Client.PostAsJsonAsync("/api/v1/column",
            new { BoardId = boardId, Name = name, Order = order });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateColumn response was null.");
        return result.Result.Value;
    }
}
