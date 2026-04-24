using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Users;

public class GetUserProductivityQueryHandler : IRequestHandler<GetUserProductivityQuery, ProductivityDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProductivityQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ProductivityDto?> Handle(GetUserProductivityQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(request.UserId);
        if (user == null)
        {
            return null;
        }

        var history = await _userRepository.GetProductivityHistory(request.UserId);

        return new ProductivityDto
        {
            CurrentScore = user.ProductivityScore,
            History = history.Select(h => new ProductivityHistoryPoint
            {
                Date = (DateTime)((dynamic)h).Date,
                Score = (decimal)((dynamic)h).Score
            }).ToList()
        };
    }
}
