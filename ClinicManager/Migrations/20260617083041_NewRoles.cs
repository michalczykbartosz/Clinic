using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class NewRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "DoctorId",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "DoctorId", "BirthDate", "FirstName", "LastName", "PESEL", "PwzNumber", "Specialization" },
                values: new object[,]
                {
                    { 1, new DateOnly(1975, 8, 19), "Adam", "Wiśniewski", "75081911223", "1234567", "Kardiolog" },
                    { 2, new DateOnly(1982, 3, 15), "Ewa", "Kowalczyk", "82031509876", "7654321", "Neurolog" }
                });
        }
    }
}
