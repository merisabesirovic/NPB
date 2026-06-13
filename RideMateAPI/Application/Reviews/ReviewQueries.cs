using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Reviews
{
	public record GetUserReviewsQuery(Guid UserId) : IRequest<List<ReviewDto>>;
	public record GetMyReviewsQuery(Guid UserId) : IRequest<List<ReviewDto>>;
	public record GetMyWrittenReviewsQuery(Guid UserId) : IRequest<List<ReviewDto>>;

	public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, List<ReviewDto>>
	{
		private readonly ReviewService _reviewService;

		public GetUserReviewsQueryHandler(ReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		public Task<List<ReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
		{
			return _reviewService.GetUserReviewsAsync(request.UserId);
		}
	}

	public class GetMyReviewsQueryHandler : IRequestHandler<GetMyReviewsQuery, List<ReviewDto>>
	{
		private readonly ReviewService _reviewService;

		public GetMyReviewsQueryHandler(ReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		public Task<List<ReviewDto>> Handle(GetMyReviewsQuery request, CancellationToken cancellationToken)
		{
			return _reviewService.GetMyReviewsAsync(request.UserId);
		}
	}

	public class GetMyWrittenReviewsQueryHandler : IRequestHandler<GetMyWrittenReviewsQuery, List<ReviewDto>>
	{
		private readonly ReviewService _reviewService;

		public GetMyWrittenReviewsQueryHandler(ReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		public Task<List<ReviewDto>> Handle(GetMyWrittenReviewsQuery request, CancellationToken cancellationToken)
		{
			return _reviewService.GetMyWrittenReviewsAsync(request.UserId);
		}
	}
}
