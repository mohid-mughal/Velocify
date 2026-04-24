using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Tasks;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICommentSentimentService _sentimentService;

    public CreateCommentCommandHandler(
        ITaskRepository taskRepository,
        ICommentSentimentService sentimentService)
    {
        _taskRepository = taskRepository;
        _sentimentService = sentimentService;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify task exists
        var task = await _taskRepository.GetById(request.TaskItemId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.TaskItemId} not found");
        }

        // Create the comment
        var comment = await _taskRepository.CreateComment(
            request.TaskItemId,
            request.Content,
            request.UserId);

        // Trigger async sentiment analysis (fire and forget)
        // REQUIREMENT 14.5: Non-blocking operation that does not delay comment creation response
        _ = Task.Run(async () =>
        {
            try
            {
                var sentimentScore = await _sentimentService.AnalyzeSentiment(request.Content);
                
                // REQUIREMENT 14.2: Store the score in the SentimentScore column
                await _taskRepository.UpdateCommentSentiment(comment.Id, sentimentScore);
            }
            catch
            {
                // Sentiment analysis failure should not affect comment creation
                // REQUIREMENT 14.5: Non-blocking operation
                // Log the error but don't throw
            }
        }, cancellationToken);

        return comment;
    }
}
