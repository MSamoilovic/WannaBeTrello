namespace Feezbow.Domain.Enums;

/// <summary>
/// Identifies which kind of entity an Attachment is linked to. OwnerId is interpreted
/// in the context of this type — for <see cref="Household"/> it is the ProjectId.
/// </summary>
public enum AttachmentOwnerType
{
    Bill = 1,
    Household = 2
}
