namespace Ecommerce.Catalog.Api.Data;

// Seed data (was a Marten IInitialData). Ids are kept so existing Postman requests and Discount coupons still match.
public static class CatalogInitialData
{
    private const string PhoneDescription =
        "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.";

    public static IEnumerable<Product> Products =>
    [
        new() { Id = new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"), Name = "IPhone X", Description = PhoneDescription, ImageFile = "product-1.png", Price = 950.00M, Stock = 25, Category = ["Smart Phone"] },
        new() { Id = new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"), Name = "Samsung 10", Description = PhoneDescription, ImageFile = "product-2.png", Price = 840.00M, Stock = 30, Category = ["Smart Phone"] },
        new() { Id = new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"), Name = "Huawei Plus", Description = PhoneDescription, ImageFile = "product-3.png", Price = 650.00M, Stock = 15, Category = ["White Appliances"] },
        new() { Id = new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"), Name = "Xiaomi Mi 9", Description = PhoneDescription, ImageFile = "product-4.png", Price = 470.00M, Stock = 40, Category = ["White Appliances"] },
        new() { Id = new Guid("b786103d-c621-4f5a-b498-23452610f88c"), Name = "HTC U11+ Plus", Description = PhoneDescription, ImageFile = "product-5.png", Price = 380.00M, Stock = 10, Category = ["Smart Phone"] },
        new() { Id = new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff"), Name = "LG G7 ThinQ", Description = PhoneDescription, ImageFile = "product-6.png", Price = 240.00M, Stock = 20, Category = ["Home Kitchen"] },
        new() { Id = new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188"), Name = "Panasonic Lumix", Description = PhoneDescription, ImageFile = "product-7.png", Price = 240.00M, Stock = 5, Category = ["Camera"] },
    ];
}
