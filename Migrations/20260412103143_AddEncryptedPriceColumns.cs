using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectsDashboards.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedPriceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VOAmount",
                table: "VariationOrders");

            migrationBuilder.DropColumn(
                name: "ContractValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ClaimAmount",
                table: "PaymentClaims");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedVOAmount",
                table: "VariationOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedContractValue",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedClaimAmount",
                table: "PaymentClaims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedVOAmount",
                table: "VariationOrders");

            migrationBuilder.DropColumn(
                name: "EncryptedContractValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EncryptedClaimAmount",
                table: "PaymentClaims");

            migrationBuilder.AddColumn<decimal>(
                name: "VOAmount",
                table: "VariationOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValue",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ClaimAmount",
                table: "PaymentClaims",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
