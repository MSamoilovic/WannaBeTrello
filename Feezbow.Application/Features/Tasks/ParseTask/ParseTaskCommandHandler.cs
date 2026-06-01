using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;
using MediatR;

namespace Feezbow.Application.Features.Tasks.ParseTask;

public class ParseTaskCommandHandler(
    IAIService aiService,
    IProjectService projectService,
    ICurrentUserService currentUserService
) : IRequestHandler<ParseTaskCommand, ParseTaskCommandResponse>
{
    private const string _systemPromptTemplate = """
        You are a task creation assistant. Today is {today}.
        Extract from the user's message:
        - title: short imperative sentence
        - assigneeHint: person's name if mentioned, else null
        - dueDate: ISO 8601 date only (YYYY-MM-DD), resolve relative references using today's date, else null
        - priority: Low | Medium | High | Urgent (default Medium)
        - description: additional context, else empty string

        Return ONLY valid JSON:
        { "title": string, "assigneeHint": string|null, "dueDate": string|null, "priority": string, "description": string }
        """;

    private record RawProposal(
        string? Title,
        string? AssigneeHint,
        string? DueDate,
        string? Priority,
        string? Description
    );

    public async Task<ParseTaskCommandResponse> Handle(
        ParseTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        // Inject today's date — caching disabled because date changes daily
        var systemPrompt = _systemPromptTemplate.Replace(
            "{today}",
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        );

        var json = await aiService.CompleteAsync(
            systemPrompt,
            request.FreeText,
            new AIRequestOptions(
                MaxInputTokens: 4_000,
                MaxOutputTokens: 512,
                AllowCaching: false,
                RequiresFeature: "TaskParser"
            ),
            cancellationToken
        );

        // Strip markdown code fences if present
        json = json.Trim();
        if (json.StartsWith("```"))
            json = string.Join('\n', json.Split('\n')[1..^1]);

        RawProposal raw;
        try
        {
            raw = System.Text.Json.JsonSerializer.Deserialize<RawProposal>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
        catch
        {
            throw new AIResponseParseException(nameof(ParseTaskProposalDto));
        }

        // Fuzzy-match assignee against project members
        long? assigneeUserId = null;
        string? assigneeName = null;

        if (!string.IsNullOrWhiteSpace(raw.AssigneeHint))
        {
            Project project = await projectService.GetProjectByIdAsync(
                request.ProjectId,
                currentUserService.UserId ?? throw new UnauthorizedAccessException(),
                cancellationToken
            );

            ProjectMember? match = project.ProjectMembers.FirstOrDefault(m =>
                m.User.DisplayName.Contains(raw.AssigneeHint, StringComparison.OrdinalIgnoreCase)
            );

            if (match is not null)
            {
                assigneeUserId = match.UserId;
                assigneeName = match.User.DisplayName;
            }
        }

        // Map priority string → enum (default Medium on unknown value)
        var priority = Enum.TryParse<TaskPriority>(raw.Priority, ignoreCase: true, out var parsed)
            ? parsed
            : TaskPriority.Medium;

        DateOnly? dueDate = DateOnly.TryParse(raw.DueDate, out var date) ? date : null;

        var proposal = new ParseTaskProposalDto(
            Title: raw.Title ?? string.Empty,
            Description: raw.Description ?? string.Empty,
            Priority: priority,
            DueDate: dueDate,
            AssigneeUserId: assigneeUserId,
            AssigneeName: assigneeName
        );

        return new ParseTaskCommandResponse(proposal);
    }
}
