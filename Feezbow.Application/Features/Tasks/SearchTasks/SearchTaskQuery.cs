using MediatR;

namespace Feezbow.Application.Features.Tasks.SearchTasks
{
    public class SearchTaskQuery: IRequest<IQueryable<SearchTaskQueryResponse>>{}
}
