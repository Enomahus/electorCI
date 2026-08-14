using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class Fix_MajContextPollingStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollingStations_Districts_DistrictDaoId",
                table: "PollingStations");

            migrationBuilder.DropIndex(
                name: "IX_PollingStations_DistrictDaoId",
                table: "PollingStations");

            migrationBuilder.DropColumn(
                name: "DistrictDaoId",
                table: "PollingStations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DistrictDaoId",
                table: "PollingStations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PollingStations_DistrictDaoId",
                table: "PollingStations",
                column: "DistrictDaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PollingStations_Districts_DistrictDaoId",
                table: "PollingStations",
                column: "DistrictDaoId",
                principalTable: "Districts",
                principalColumn: "Id");
        }
    }
}
