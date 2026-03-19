using MediatR;

namespace Feezbow.Application.Features.Comments.RestoreComment;

public record RestoreCommentCommand(long CommentId) : IRequest<RestoreCommentCommandResponse>;
