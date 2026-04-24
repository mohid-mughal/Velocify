using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskCommentsQuery : IRequest<List<CommentDto>>
{
    public Guid TaskItemId { get; set; }
}
