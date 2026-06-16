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
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = N'1' OR [NormalizedName] = N'PACJENT')
                BEGIN
                    INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
                    VALUES (N'1', N'test', N'Pacjent', N'PACJENT');
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = N'2' OR [NormalizedName] = N'ADMIN')
                BEGIN
                    INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
                    VALUES (N'2', N'test', N'Admin', N'ADMIN');
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Visits] WHERE [VisitId] = 1)
                BEGIN
                    SET IDENTITY_INSERT [Visits] ON;
                    INSERT INTO [Visits] ([VisitId], [DoctorId], [PatientId], [VisitDateTime], [VisitStatus])
                    VALUES (1, 1, 1, '2026-06-15T14:00:00.0000000', 0);
                    SET IDENTITY_INSERT [Visits] OFF;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Visits] WHERE [VisitId] = 2)
                BEGIN
                    SET IDENTITY_INSERT [Visits] ON;
                    INSERT INTO [Visits] ([VisitId], [DoctorId], [PatientId], [VisitDateTime], [VisitStatus])
                    VALUES (2, 2, 1, '2026-06-10T10:30:00.0000000', 1);
                    SET IDENTITY_INSERT [Visits] OFF;
                END
                """);
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
