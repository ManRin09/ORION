using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    auth_provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Local"),
                    github_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "User"),
                    plan_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Free"),
                    accepted_terms_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    current_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    longest_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gems = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_gamification_non_negative", "total_xp >= 0 AND current_level >= 1 AND current_streak >= 0 AND longest_streak >= 0 AND gems >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_github_id",
                table: "users",
                column: "github_id",
                unique: true,
                filter: "github_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
