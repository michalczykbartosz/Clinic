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
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Procedures_Visits_VisitId')
                BEGIN
                    ALTER TABLE [Procedures] DROP CONSTRAINT [FK_Procedures_Visits_VisitId];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'Name') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    SELECT @constraintName = [dc].[name]
                    FROM sys.default_constraints [dc]
                    INNER JOIN sys.columns [c] ON [dc].[parent_object_id] = [c].[object_id] AND [dc].[parent_column_id] = [c].[column_id]
                    WHERE [dc].[parent_object_id] = OBJECT_ID(N'dbo.Procedures') AND [c].[name] = N'Name';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [Procedures] DROP CONSTRAINT [' + @constraintName + N']');
                    END

                    ALTER TABLE [Procedures] DROP COLUMN [Name];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NULL
                BEGIN
                    EXEC sp_rename N'[Procedures].[VisitId]', N'VisitId1', N'COLUMN';
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId1' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                BEGIN
                    EXEC sp_rename N'[Procedures].[IX_Procedures_VisitId]', N'IX_Procedures_VisitId1', N'INDEX';
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Procedures_Visits_VisitId1')
                BEGIN
                    ALTER TABLE [Procedures] DROP CONSTRAINT [FK_Procedures_Visits_VisitId1];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId1') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Procedures', N'VisitId') IS NULL
                BEGIN
                    EXEC sp_rename N'[Procedures].[VisitId1]', N'VisitId', N'COLUMN';
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId1' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Procedures_VisitId' AND [object_id] = OBJECT_ID(N'dbo.Procedures'))
                BEGIN
                    EXEC sp_rename N'[Procedures].[IX_Procedures_VisitId1]', N'IX_Procedures_VisitId', N'INDEX';
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'Name') IS NULL
                BEGIN
                    ALTER TABLE [Procedures]
                    ADD [Name] nvarchar(max) NOT NULL CONSTRAINT [DF_Procedures_Name_RemoveVisitProcedureRelation] DEFAULT N'';
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Procedures', N'VisitId') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Procedures_Visits_VisitId')
                BEGIN
                    ALTER TABLE [Procedures]
                    ADD CONSTRAINT [FK_Procedures_Visits_VisitId]
                    FOREIGN KEY ([VisitId]) REFERENCES [Visits] ([VisitId]);
                END
                """);
        }
    }
}
