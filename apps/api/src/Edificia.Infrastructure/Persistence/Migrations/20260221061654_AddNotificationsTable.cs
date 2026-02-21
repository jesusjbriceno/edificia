using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS so the migration is idempotent when the table was
            // already created outside of EF migrations tracking.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS notifications (
                    id           uuid                        NOT NULL,
                    title        character varying(200)      NOT NULL,
                    message      character varying(1000)     NOT NULL,
                    is_read      boolean                     NOT NULL DEFAULT false,
                    user_id      uuid                        NOT NULL,
                    created_at   timestamp with time zone    NOT NULL,
                    updated_at   timestamp with time zone,
                    CONSTRAINT pk_notifications PRIMARY KEY (id)
                );
                CREATE INDEX IF NOT EXISTS ix_notifications_user_id ON notifications (user_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
