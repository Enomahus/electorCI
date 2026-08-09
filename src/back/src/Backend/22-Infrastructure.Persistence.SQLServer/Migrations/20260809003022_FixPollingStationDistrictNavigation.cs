using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class FixPollingStationDistrictNavigation : Migration
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

            migrationBuilder.AlterColumn<Guid>(
                name: "AuthorId",
                table: "RegistrationRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AuthorId",
                table: "RegistrationRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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
