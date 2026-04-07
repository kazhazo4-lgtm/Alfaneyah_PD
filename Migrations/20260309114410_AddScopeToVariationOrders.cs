using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectsDashboards.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeToVariationOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "VariationOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scope",
                table: "VariationOrders");
        }
    }
}
