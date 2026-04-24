using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskAuditLogQuery : IRequest<List<TaskAuditLogDto>>
{
    public Guid TaskItemId { get; set; }
}
