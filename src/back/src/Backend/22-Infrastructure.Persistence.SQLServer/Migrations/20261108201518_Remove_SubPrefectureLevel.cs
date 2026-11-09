using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class Remove_SubPrefectureLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 146L,
                column: "Level",
                value: "Municipality");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 152L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 153L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001002", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 154L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001003", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 155L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001004", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 156L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001005", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 157L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001006", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 158L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001007", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 159L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001008", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 160L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001009", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 161L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001010", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 162L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001011", 146L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 163L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001012", 146L });

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 147L);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 148L);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 149L);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 150L);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 151L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "Code", "CreatedAt", "DisabledDate", "Level", "ModifiedAt", "ParentId", "Wording" },
                values: new object[,]
                {
                    { 147L, "005001002", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "SubPrefecture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 17L, "Didiévi" },
                    { 148L, "005001003", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "SubPrefecture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 17L, "Molonou-blé" },
                    { 149L, "005001004", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "SubPrefecture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 17L, "Raviart" },
                    { 150L, "005001005", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "SubPrefecture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 17L, "Tié-N'diekro" },
                    { 151L, "005001001001", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Municipality", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 146L, "Boli" }
                });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 146L,
                column: "Level",
                value: "SubPrefecture");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 152L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001001", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 153L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001002", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 154L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001003", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 155L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001004", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 156L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001005", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 157L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001006", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 158L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001007", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 159L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001008", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 160L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001009", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 161L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001010", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 162L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001011", 151L });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 163L,
                columns: new[] { "Code", "ParentId" },
                values: new object[] { "005001001001012", 151L });
        }
    }
}
