using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sashiel_ST10028058_PROG6212_Part2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFinalPaymentCalc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Claims");
        }
    }
}
