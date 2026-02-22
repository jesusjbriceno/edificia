using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La columna, el índice y la clave foránea ya existen en la base de datos de PostgreSQL.
            // Dejamos este método vacío para que EF Core registre la migración sin intentar recrearlos.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dejamos vacío el Down por la misma razón.
        }
    }
}
