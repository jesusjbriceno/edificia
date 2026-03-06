using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "template_params",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    formatter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_params", x => x.id);
                    table.CheckConstraint("CK_TemplateParams_Key_NotEmpty", "key <> ''");
                    table.CheckConstraint("CK_TemplateParams_SourceCode_NotEmpty", "source_code <> ''");
                });

            migrationBuilder.CreateIndex(
                name: "ix_template_params_is_active",
                table: "template_params",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_template_params_key",
                table: "template_params",
                column: "key",
                unique: true);

            migrationBuilder.InsertData(
                table: "template_params",
                columns: new[] { "id", "created_at", "display_name", "formatter", "is_active", "key", "source_code", "updated_at" },
                values: new object[,]
                {
                    { new Guid("7b75c505-8cc5-4d23-a577-51c0f31957ec"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Titulo del proyecto", null, true, "PROJECT_TITLE", "PROJECT_TITLE", null },
                    { new Guid("8f2ab4ec-0cd8-4f79-a11d-cf4f2f388f39"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Descripcion del proyecto", null, true, "PROJECT_DESCRIPTION", "PROJECT_DESCRIPTION", null },
                    { new Guid("7a6a6c6a-6510-4627-a89c-3945c5898f4a"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Direccion del proyecto", null, true, "PROJECT_ADDRESS", "PROJECT_ADDRESS", null },
                    { new Guid("bc71f3ab-7658-43f8-a333-a24eb57faff1"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Tipo de intervencion", null, true, "INTERVENTION_TYPE", "INTERVENTION_TYPE", null },
                    { new Guid("428c1f1c-ae91-4df5-a96f-8da6ed6298f0"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "LOE requerida", null, true, "IS_LOE_REQUIRED", "IS_LOE_REQUIRED", null },
                    { new Guid("8bcf4f66-e3f7-4be9-9f5e-a42131f6d632"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Referencia catastral", null, true, "CADASTRAL_REFERENCE", "CADASTRAL_REFERENCE", null },
                    { new Guid("f1e87d8f-7480-4b5d-9e62-18235f37af0f"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Normativa local", null, true, "LOCAL_REGULATIONS", "LOCAL_REGULATIONS", null },
                    { new Guid("23b6f467-2f6f-4d7b-be3b-a9954e94ff89"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Fecha de exportacion", null, true, "EXPORT_DATE", "EXPORT_DATE", null },
                    { new Guid("f9fc09bb-85bd-42b9-8de6-4f14ea8cb0f3"), new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), "Fecha y hora de exportacion", null, true, "EXPORT_DATETIME", "EXPORT_DATETIME", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("7b75c505-8cc5-4d23-a577-51c0f31957ec"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("8f2ab4ec-0cd8-4f79-a11d-cf4f2f388f39"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("7a6a6c6a-6510-4627-a89c-3945c5898f4a"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("bc71f3ab-7658-43f8-a333-a24eb57faff1"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("428c1f1c-ae91-4df5-a96f-8da6ed6298f0"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("8bcf4f66-e3f7-4be9-9f5e-a42131f6d632"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("f1e87d8f-7480-4b5d-9e62-18235f37af0f"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("23b6f467-2f6f-4d7b-be3b-a9954e94ff89"));

            migrationBuilder.DeleteData(
                table: "template_params",
                keyColumn: "id",
                keyValue: new Guid("f9fc09bb-85bd-42b9-8de6-4f14ea8cb0f3"));

            migrationBuilder.DropTable(
                name: "template_params");
        }
    }
}
