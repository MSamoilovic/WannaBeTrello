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
    BoardArchived,
    BoardRestored,
    TaskCreated,
    TaskUpdated,
    TaskMoved,
    TaskCompleted,
    CommentAdded,
    BoardDeleted,
    ColumnAdded,
    ColumnUpdated,
    ColumnDeleted,
    ColumnOrderChanged,
    ColumnWipLimitChanged,
    UserInvited,
    UserJoinedBoard,
    UserRemovedFromBoard,
}