using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class SeedRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", "test", "Pacjent", "PACJENT" },
                    { "2", "test", "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Visits",
                columns: new[] { "VisitId", "DoctorId", "PatientId", "VisitDateTime", "VisitStatus" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2026, 6, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), 0 },
                    { 2, 2, 1, new DateTime(2026, 6, 10, 10, 30, 0, 0, DateTimeKind.Unspecified), 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "Visits",
                keyColumn: "VisitId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Visits",
                keyColumn: "VisitId",
                keyValue: 2);
        }
    }
}
