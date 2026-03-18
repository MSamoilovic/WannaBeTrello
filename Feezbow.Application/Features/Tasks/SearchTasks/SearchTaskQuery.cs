using MediatR;

namespace WannabeTrello.Application.Features.Tasks.SearchTasks
{
    public class SearchTaskQuery: IRequest<IQueryable<SearchTaskQueryResponse>>{}
}
