using MediatR;

namespace Feezbow.Application.Features.Comments.DeleteComment;

public record DeleteCommentCommand(long CommentId) : IRequest<DeleteCommentCommandResponse>;
