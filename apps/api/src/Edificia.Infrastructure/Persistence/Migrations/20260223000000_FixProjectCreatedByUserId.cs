using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edificia.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Corrección de la migración 20260222162605_AddProjectCreatedByUserId cuyo Up() quedó
    /// vacío, por lo que la columna created_by_user_id nunca se creó en producción.
    /// Utiliza SQL idempotente (IF NOT EXISTS) para no fallar en entornos locales
    /// donde la columna ya existía previamente.
    /// </summary>
    public partial class FixProjectCreatedByUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Añadir columna (idempotente)
            migrationBuilder.Sql(@"
                ALTER TABLE projects
                ADD COLUMN IF NOT EXISTS created_by_user_id uuid NOT NULL
                    DEFAULT '00000000-0000-0000-0000-000000000000';
            ");

            // 2. Crear índice (idempotente)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_projects_created_by_user_id
                ON projects (created_by_user_id);
            ");

            // 3. Añadir FK solo si no existe ya
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'fk_projects_asp_net_users_created_by_user_id'
                    ) THEN
                        ALTER TABLE projects
                        ADD CONSTRAINT fk_projects_asp_net_users_created_by_user_id
                        FOREIGN KEY (created_by_user_id)
                        REFERENCES asp_net_users (id)
                        ON DELETE RESTRICT;
                    END IF;
                END
                $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE projects
                DROP CONSTRAINT IF EXISTS fk_projects_asp_net_users_created_by_user_id;
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_projects_created_by_user_id;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE projects
                DROP COLUMN IF EXISTS created_by_user_id;
            ");
        }
    }
}
