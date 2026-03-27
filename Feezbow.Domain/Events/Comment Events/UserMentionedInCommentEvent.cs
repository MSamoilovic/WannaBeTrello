namespace Feezbow.Domain.Events.Comment_Events;

public class UserMentionedInCommentEvent(
    long taskId,
    long mentionedByUserId,
    IReadOnlySet<string> mentionedUsernames)
    : DomainEvent
{
    public long TaskId { get; } = taskId;
    public long MentionedByUserId { get; } = mentionedByUserId;
    public IReadOnlySet<string> MentionedUsernames { get; } = mentionedUsernames;
}
