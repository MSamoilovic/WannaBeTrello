using System.Net;
using System.Net.Http.Json;
using Feezbow.Application.Features.Projects.AddProjectMember;
using Feezbow.Application.Features.Projects.UpdateProject;
using Feezbow.Domain.Enums;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Projects;

[Collection("Integration")]
public class ProjectControllerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateProject_WithValidData_ReturnsCreated()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = "Test Project", Description = "Description" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = "Test Project", Description = (string?)null });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectById_ExistingProject_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var response = await Client.GetAsync($"/api/v1/project/{projectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectById_NonExistentId_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var response = await Client.GetAsync("/api/v1/project/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var command = new UpdateProjectCommand(
            projectId, "Updated Name", "Updated Desc",
            ProjectStatus.Active, ProjectVisibility.Private, false);

        var response = await Client.PutAsJsonAsync($"/api/v1/project/{projectId}", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProject_IdMismatch_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var command = new UpdateProjectCommand(
            projectId + 1, "Name", null,
            ProjectStatus.Active, ProjectVisibility.Private, false);

        var response = await Client.PutAsJsonAsync($"/api/v1/project/{projectId}", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveProject_ExistingProject_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var response = await Client.PostAsync($"/api/v1/project/{projectId}/archive", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RestoreProject_AfterArchive_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        await Client.PostAsync($"/api/v1/project/{projectId}/archive", null);
        var response = await Client.PostAsync($"/api/v1/project/{projectId}/restore", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddProjectMember_WithValidUser_ReturnsOk()
    {
        var ownerToken = await RegisterAndLoginAsync();
        SetAuthToken(ownerToken);
        var projectId = await CreateProjectAsync();

        // Kreiraj drugog korisnika i dohvati njegov ID
        var memberToken = await RegisterAndLoginAsync();
        SetAuthToken(memberToken);
        var memberId = await GetCurrentUserIdAsync();

        // Vrati se na vlasnika i dodaj membera
        SetAuthToken(ownerToken);
        var command = new AddProjectMemberCommand(projectId, memberId, ProjectRole.Contributor);
        var response = await Client.PostAsJsonAsync($"/api/v1/project/{projectId}/add-member", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectMembers_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var projectId = await CreateProjectAsync();

        var response = await Client.GetAsync($"/api/v1/project/{projectId}/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // --- helpers ---

    private async Task<long> CreateProjectAsync(string name = "Test Project")
    {
        var response = await Client.PostAsJsonAsync("/api/v1/project",
            new { Name = name, Description = (string?)null });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateResponse>()
            ?? throw new InvalidOperationException("CreateProject response was null.");
        return result.Result.Value;
    }
}
