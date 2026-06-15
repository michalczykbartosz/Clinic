using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Procedures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "Procedures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_VisitId",
                table: "Procedures",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Procedures_Visits_VisitId",
                table: "Procedures",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Procedures_Visits_VisitId",
                table: "Procedures");

            migrationBuilder.DropIndex(
                name: "IX_Procedures_VisitId",
                table: "Procedures");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Procedures");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "Procedures");
        }
    }
}
