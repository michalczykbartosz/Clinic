using ClinicManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ClinicDbContext))]
    [Migration("20260616174000_RemoveProcedureVisitShadowColumn")]
    public partial class RemoveProcedureVisitShadowColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Procedures_Visits_VisitId1')
                BEGIN
                    ALTER TABLE [Procedures] DROP CONSTRAINT [FK_Procedures_Visits_VisitId1];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId1' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                BEGIN
                    DROP INDEX [IX_Procedures_VisitId1] ON [Procedures];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NOT NULL
                BEGIN
                    ALTER TABLE [Procedures] DROP COLUMN [VisitId1];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NULL
                BEGIN
                    ALTER TABLE [Procedures] ADD [VisitId1] int NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId1' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                BEGIN
                    CREATE INDEX [IX_Procedures_VisitId1] ON [Procedures] ([VisitId1]);
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Procedures_Visits_VisitId1')
                BEGIN
                    ALTER TABLE [Procedures]
                    ADD CONSTRAINT [FK_Procedures_Visits_VisitId1]
                    FOREIGN KEY ([VisitId1]) REFERENCES [Visits] ([VisitId]);
                END
                """);
        }
    }
}
