using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVisitProcedureRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Procedures_Visits_VisitId",
                table: "Procedures");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Procedures");

            migrationBuilder.RenameColumn(
                name: "VisitId",
                table: "Procedures",
                newName: "VisitId1");

            migrationBuilder.RenameIndex(
                name: "IX_Procedures_VisitId",
                table: "Procedures",
                newName: "IX_Procedures_VisitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Procedures_Visits_VisitId1",
                table: "Procedures",
                column: "VisitId1",
                principalTable: "Visits",
                principalColumn: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Procedures_Visits_VisitId1",
                table: "Procedures");

            migrationBuilder.RenameColumn(
                name: "VisitId1",
                table: "Procedures",
                newName: "VisitId");

            migrationBuilder.RenameIndex(
                name: "IX_Procedures_VisitId1",
                table: "Procedures",
                newName: "IX_Procedures_VisitId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Procedures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Procedures_Visits_VisitId",
                table: "Procedures",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "VisitId");
        }
    }
}
