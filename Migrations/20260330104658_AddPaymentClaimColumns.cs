using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectsDashboards.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentClaimColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentClaims",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClaimDate",
                table: "PaymentClaims",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "PaymentClaims",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthPayment",
                table: "PaymentClaims",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VATDate",
                table: "PaymentClaims",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 2,
                column: "FullName",
                value: "Accountant");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 3,
                column: "FullName",
                value: "Staff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "PaymentClaims");

            migrationBuilder.DropColumn(
                name: "MonthPayment",
                table: "PaymentClaims");

            migrationBuilder.DropColumn(
                name: "VATDate",
                table: "PaymentClaims");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PaymentClaims",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClaimDate",
                table: "PaymentClaims",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 2,
                column: "FullName",
                value: "John Accountant");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 3,
                column: "FullName",
                value: "Sarah Staff");
        }
    }
}
