using Moq;
using WannabeTrello.Application.Features.Projects.GetProjectMembersById;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class GetProjectMembersByIdQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly GetProjectMembersByIdQueryHandler _handler;

    public GetProjectMembersByIdQueryHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _handler = new GetProjectMembersByIdQueryHandler(_projectRepositoryMock.Object);
    }
    
    //TODO: Apdejtovati User Entitet i onda promeniti kreiranje usera unutar ProjectMember
    // [Fact]
    // public async Task Handle_WithExistingMembers_ReturnsListOfMembers()
    // {
    //     // ARRANGE
    //     const int projectId = 1;
    //     const long ownerUserId = 1;
    //     const long contributorUserId = 2;
    //     
    //     // Create mock data
    //     var projectMembers = new List<ProjectMember>
    //     {
    //         ProjectMember.Create(ownerUserId, projectId, ProjectRole.Owner),
    //         ProjectMember.Create(contributorUserId, projectId, ProjectRole.Contributor)
    //     };
    //     
    //     _projectRepositoryMock.Setup(x => x.GetProjectMembersByIdAsync(projectId))
    //                           .ReturnsAsync(projectMembers);
    //
    //     var query = new GetProjectMembersByIdQuery(projectId);
    //
    //     // ACT
    //     var result = await _handler.Handle(query, CancellationToken.None);
    //
    //     // ASSERT
    //     Assert.NotNull(result);
    //     Assert.Equal(2, result.Count);
    //
    //     var firstMember = result.First();
    //     Assert.Equal(1, firstMember.UserId);
    //     Assert.Equal("John", firstMember.FirstName);
    //     Assert.Equal("Doe", firstMember.LastName);
    //     Assert.Equal(ProjectRole.Owner, firstMember.Role);
    //     
    //     var secondMember = result.Last();
    //     Assert.Equal(2, secondMember.UserId);
    //     Assert.Equal("Jane", secondMember.FirstName);
    //     Assert.Equal("Doe", secondMember.LastName);
    //     Assert.Equal(ProjectRole.Contributor, secondMember.Role);
    //     
    //     _projectRepositoryMock.Verify(x => x.GetProjectMembersByIdAsync(projectId), Times.Once);
    // }

    [Fact]
    public async Task Handle_ProjectHasNoMembers_ReturnsEmptyList()
    {
        // ARRANGE
        var projectId = 123;
        
        // Mock the repository to return an empty list
        _projectRepositoryMock.Setup(x => x.GetProjectMembersByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new List<ProjectMember>());

        var query = new GetProjectMembersByIdQuery(projectId);

        // ACT
        var result = await _handler.Handle(query, CancellationToken.None);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);
        _projectRepositoryMock.Verify(x => x.GetProjectMembersByProjectIdAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task Handle_ProjectDoesNotExist_ReturnsEmptyList()
    {
        // ARRANGE
        var nonExistentProjectId = 999;
        
        // Mock the repository to return null for a non-existent project
        _projectRepositoryMock.Setup(x => x.GetProjectMembersByProjectIdAsync(nonExistentProjectId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync([]);

        var query = new GetProjectMembersByIdQuery(nonExistentProjectId);

        // ACT
        var result = await _handler.Handle(query, CancellationToken.None);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);
        _projectRepositoryMock.Verify(x => x.GetProjectMembersByProjectIdAsync(nonExistentProjectId, It.IsAny<CancellationToken>()), Times.Once);
    }
}