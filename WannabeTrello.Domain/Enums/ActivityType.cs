namespace WannabeTrello.Domain.Enums;

public enum ActivityType
{
    ProjectCreated,
    ProjectUpdated,
    ProjectArchived,
    ProjectMemberAdded,
    ProjectMemberRoleUpdated,
    ProjectMemberRemoved,
    BoardCreated,
    BoardUpdated,
    TaskCreated,
    TaskMoved,
    TaskCompleted,
    CommentAdded,
    BoardDeleted,
    ColumnAdded,
    ColumnUpdated,
    ColumnDeleted,
    UserInvited,
    UserJoinedBoard,
    UserRemovedFromBoard,
}