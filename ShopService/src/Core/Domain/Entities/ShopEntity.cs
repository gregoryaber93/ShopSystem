namespace ShopService.Domain.Entities;

public class ShopEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? OwnerUserId { get; set; }
}
