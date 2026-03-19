using MediatR;

namespace Feezbow.Application.Features.Comments.UpdateCommentContent;

public record UpdateCommentContentCommand(long CommentId, string? NewContent): IRequest<UpdateCommentContentCommandResponse>;