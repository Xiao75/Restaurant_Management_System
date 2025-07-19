using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDateToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Payments__OrderI__44FF419A",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Payments__9B556A583FAD8793",
                table: "Payments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Payments__OrderID",
                table: "Payments",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Payments__OrderID",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Payments__9B556A583FAD8793",
                table: "Payments",
                column: "PaymentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Payments__OrderI__44FF419A",
                table: "Payments",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID");
        }
    }
}
