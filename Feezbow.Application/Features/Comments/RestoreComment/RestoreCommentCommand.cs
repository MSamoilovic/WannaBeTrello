using MediatR;

namespace WannabeTrello.Application.Features.Comments.RestoreComment;

public record RestoreCommentCommand(long CommentId) : IRequest<RestoreCommentCommandResponse>;
