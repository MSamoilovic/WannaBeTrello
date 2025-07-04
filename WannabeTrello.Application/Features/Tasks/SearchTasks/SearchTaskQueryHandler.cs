using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Tasks.SearchTasks
{
    internal class SearchTaskQueryHandler(IBoardTaskRepository taskRepository, ICurrentUserService currentUserService) : IRequestHandler<SearchTaskQuery, IQueryable<SearchTaskQueryResponse>>
    {
        public async Task<IQueryable<SearchTaskQueryResponse>> Handle(SearchTaskQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
            }


            return taskRepository.SearchTask().Select(SearchTaskQueryResponse.FromEntity).AsQueryable();
        }
    }
}
