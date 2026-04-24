using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Queries.Users;

public class GetUserProductivityQuery : IRequest<ProductivityDto?>
{
    public Guid UserId { get; set; }
}
