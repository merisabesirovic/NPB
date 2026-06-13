using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Reviews
{
	public record CreateReviewCommand(Guid ReviewerId, CreateReviewRequest Request) : IRequest<ReviewDto>;

	public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
	{
		private readonly ReviewService _reviewService;

		public CreateReviewCommandHandler(ReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		public Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
		{
			return _reviewService.CreateReviewAsync(request.ReviewerId, request.Request);
		}
	}
}
