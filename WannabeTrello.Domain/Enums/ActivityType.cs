namespace WannabeTrello.Domain.Enums;

public enum ActivityType
{
    ProjectCreated,
    ProjectUpdated,
    ProjectArchived,
    ProjectMemberAdded,
    ProjectMemberRoleUpdated,
    ProjectMemberRemoved,
    TaskCreated,
    TaskMoved,
    TaskCompleted,
    CommentAdded,
    BoardCreated,
    BoardUpdated,
    BoardDeleted,
    ColumnAdded,
    ColumnUpdated,
    ColumnDeleted,
    UserInvited,
    UserJoinedBoard,
    UserRemovedFromBoard,
}