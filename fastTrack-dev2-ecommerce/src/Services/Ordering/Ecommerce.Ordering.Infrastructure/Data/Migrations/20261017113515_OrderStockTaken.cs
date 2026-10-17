using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Ordering.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderStockTaken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StockTaken",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Existing orders created from a checkout already took stock (OrderFullfilment has been on since
            // checkout orders were introduced), except declined payments. Seed orders have no CheckoutId and
            // never took stock. Cancelled orders stay false: their stock can't be given back retroactively.
            migrationBuilder.Sql(
                "UPDATE Orders SET StockTaken = 1 " +
                "WHERE CheckoutId IS NOT NULL AND Status NOT IN ('PaymentFailed', 'Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockTaken",
                table: "Orders");
        }
    }
}
