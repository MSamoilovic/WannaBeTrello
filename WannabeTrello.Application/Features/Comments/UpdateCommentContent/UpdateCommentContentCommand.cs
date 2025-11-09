using MediatR;

namespace WannabeTrello.Application.Features.Comments.UpdateCommentContent;

public record UpdateCommentContentCommand(long CommentId, string? NewContent): IRequest<UpdateCommentContentCommandResponse>;