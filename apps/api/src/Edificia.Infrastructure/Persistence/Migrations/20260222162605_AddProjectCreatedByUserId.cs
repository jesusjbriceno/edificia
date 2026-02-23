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
            // La columna, el índice y la clave foránea ya existían en la BD local cuando
            // se creó esta migración. El Up() quedó vacío por error, por lo que producción
            // nunca recibió la columna. Ver migración 20260223000000_FixProjectCreatedByUserId.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
