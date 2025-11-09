using MediatR;

namespace WannabeTrello.Application.Features.Comments.DeleteComment;

public record DeleteCommentCommand(long CommentId) : IRequest<DeleteCommentCommandResponse>;
