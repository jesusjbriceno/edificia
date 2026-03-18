using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateAvailabilityAndDefaultState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string tableName = "app_templates";
            const string templateTypeColumn = "template_type";
            const string uniqueByTypeIndex = "ix_app_templates_template_type";

            migrationBuilder.DropIndex(
                name: uniqueByTypeIndex,
                table: tableName);

            migrationBuilder.DropIndex(
                name: "ix_app_templates_template_type_is_active",
                table: tableName);

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: tableName,
                newName: "is_default");

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: tableName,
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE app_templates SET is_available = is_default;");

            migrationBuilder.CreateIndex(
                name: uniqueByTypeIndex,
                table: tableName,
                column: templateTypeColumn,
                unique: true,
                filter: "is_default = true");

            migrationBuilder.CreateIndex(
                name: "ix_app_templates_template_type_is_available",
                table: tableName,
                columns: new[] { templateTypeColumn, "is_available" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            const string tableName = "app_templates";
            const string templateTypeColumn = "template_type";
            const string uniqueByTypeIndex = "ix_app_templates_template_type";

            migrationBuilder.DropIndex(
                name: uniqueByTypeIndex,
                table: tableName);

            migrationBuilder.DropIndex(
                name: "ix_app_templates_template_type_is_available",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "is_available",
                table: tableName);

            migrationBuilder.RenameColumn(
                name: "is_default",
                table: tableName,
                newName: "is_active");

            migrationBuilder.CreateIndex(
                name: uniqueByTypeIndex,
                table: tableName,
                column: templateTypeColumn,
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_app_templates_template_type_is_active",
                table: tableName,
                columns: new[] { templateTypeColumn, "is_active" });
        }
    }
}
