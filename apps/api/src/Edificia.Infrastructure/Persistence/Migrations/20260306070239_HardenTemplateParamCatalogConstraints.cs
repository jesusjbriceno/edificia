using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenTemplateParamCatalogConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateParams_Formatter_Allowed",
                table: "template_params",
                sql: "formatter IS NULL OR formatter IN ('UPPER','LOWER','TRIM')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateParams_Key_Format",
                table: "template_params",
                sql: "key ~ '^[A-Z0-9_]+$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateParams_SourceCode_Allowed",
                table: "template_params",
                sql: "source_code IN ('PROJECT_TITLE','PROJECT_DESCRIPTION','PROJECT_ADDRESS','INTERVENTION_TYPE','IS_LOE_REQUIRED','CADASTRAL_REFERENCE','LOCAL_REGULATIONS','EXPORT_DATE','EXPORT_DATETIME')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateParams_Formatter_Allowed",
                table: "template_params");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateParams_Key_Format",
                table: "template_params");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateParams_SourceCode_Allowed",
                table: "template_params");
        }
    }
}
