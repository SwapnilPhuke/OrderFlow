using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(int UserId, bool IsAdmin) : IRequest<ApiResponse<DashboardStatsDto>>;
