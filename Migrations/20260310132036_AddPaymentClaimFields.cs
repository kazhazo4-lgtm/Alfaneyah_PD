using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectsDashboards.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentClaimFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ClaimDate",
                table: "PaymentClaims",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PaymentClaims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PaymentClaims");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClaimDate",
                table: "PaymentClaims",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
