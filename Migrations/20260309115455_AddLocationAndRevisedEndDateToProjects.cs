using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectsDashboards.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationAndRevisedEndDateToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectLocation",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevisedEndDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectLocation",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RevisedEndDate",
                table: "Projects");
        }
    }
}
