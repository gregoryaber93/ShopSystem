namespace ProductService.Contracts.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Type,
    decimal Price,
    Guid ShopId);
