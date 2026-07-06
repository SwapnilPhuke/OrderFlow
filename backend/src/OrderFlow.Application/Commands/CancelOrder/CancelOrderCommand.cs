using MediatR;
using OrderFlow.Application.Common;

namespace OrderFlow.Application.Commands.CancelOrder;

public record CancelOrderCommand(
    int OrderId,
    int UserId,
    bool IsAdmin) : IRequest<ApiResponse<bool>>;
