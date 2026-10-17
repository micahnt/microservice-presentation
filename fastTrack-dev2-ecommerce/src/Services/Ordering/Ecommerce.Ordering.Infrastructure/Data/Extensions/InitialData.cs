namespace Ecommerce.Ordering.Infrastructure.Data.Extensions;

// Demo orders for the admin screen. These customers are not Identity users, so nobody can log in as them.
internal class InitialData
{
    private static readonly Guid MehmetId = new("58c49479-ec65-4de2-86e7-033c546291aa");
    private static readonly Guid JohnId = new("189dc8dc-990f-48e0-a37b-e6f2b60b9d7d");

    public static IEnumerable<Customer> Customers =>
    [
        Customer.Create(CustomerId.Of(MehmetId), "mehmet", "mehmet@seed.local"),
        Customer.Create(CustomerId.Of(JohnId), "john", "john@seed.local"),
    ];

    public static IEnumerable<Order> OrdersWithItems
    {
        get
        {
            var address1 = Address.Of("mehmet", "ozkaya", "mehmet@seed.local", "Bahcelievler No:4", "Turkey", "Istanbul", "38050");
            var address2 = Address.Of("john", "doe", "john@seed.local", "Broadway No:1", "England", "Nottingham", "08050");

            var order1 = Order.Create(
                            OrderId.Of(Guid.NewGuid()),
                            CustomerId.Of(MehmetId),
                            OrderName.Of("ORD_1"),
                            shippingAddress: address1,
                            billingAddress: address1,
                            PaymentMethod.CashOnDelivery);
            order1.Add(ProductId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")), "IPhone X", 2, 800);
            order1.Add(ProductId.Of(new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914")), "Samsung 10", 1, 740);

            var order2 = Order.Create(
                            OrderId.Of(Guid.NewGuid()),
                            CustomerId.Of(JohnId),
                            OrderName.Of("ORD_2"),
                            shippingAddress: address2,
                            billingAddress: address2,
                            PaymentMethod.FakeCard);
            order2.Add(ProductId.Of(new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8")), "Huawei Plus", 1, 650);
            order2.Add(ProductId.Of(new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27")), "Xiaomi Mi 9", 2, 470);
            order2.MarkPaid("FAKE-SEED0001");

            return [order1, order2];
        }
    }
}
