using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CltPrueba.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsdRateToCurrentQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "RateToBase",
                value: 5765m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "RateToBase",
                value: 7300m);
        }
    }
}
