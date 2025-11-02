using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.SearchTasks
{
    internal class SearchTaskQueryHandler(IBoardTaskService taskService, ICurrentUserService currentUserService) 
        : IRequestHandler<SearchTaskQuery, IQueryable<SearchTaskQueryResponse>>
    {
        public Task<IQueryable<SearchTaskQueryResponse>> Handle(SearchTaskQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            
            var tasks = taskService.SearchTasks(currentUserService.UserId.Value);
            var result = tasks.Select(SearchTaskQueryResponse.FromEntity).AsQueryable();
            
            return Task.FromResult(result);
        }
    }
}
