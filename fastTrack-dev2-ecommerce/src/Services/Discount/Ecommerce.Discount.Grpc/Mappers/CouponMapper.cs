using Ecommerce.Discount.Grpc.Models;

namespace Ecommerce.Discount.Grpc.Mappers;

public static class CouponMapper
{
    public static CouponModel ToModel(this Coupon c) => new()
    {
        Id = c.Id, ProductName = c.ProductName, Description = c.Description, Amount = c.Amount,
    };

    public static Coupon ToEntity(this CouponModel m) => new()
    {
        Id = m.Id, ProductName = m.ProductName, Description = m.Description, Amount = m.Amount,
    };
}
