/*
 * DTOs define a clean, tailored contract for what data enters and leaves your API
 */

namespace InventoryApi.DTOs;

public record OrderItemCreateDto(int ProductId, int Quantity);
public record CreateOrderRequestDto(int CustomerId, List<OrderItemCreateDto> Items);

public record OrderResponseDto(
    int OrderId,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    List<OrderItemResponseDto> Items
);

public record OrderItemResponseDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public record AnalyticsSummaryDto(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    List<TopSellingProductDto> TopProducts
);

public record TopSellingProductDto(
    int ProductId,
    string ProductName,
    int TotalUnitsSold,
    decimal RevenueGenerated
);