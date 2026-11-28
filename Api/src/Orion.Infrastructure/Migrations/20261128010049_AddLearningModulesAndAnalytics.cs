using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Orion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningModulesAndAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    visitor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    module_slug = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_analytics_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "learning_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_modules", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "learning_modules",
                columns: new[] { "id", "description", "is_active", "order_index", "slug", "title" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "Master fundamental syntax, control structures, functions, and data manipulation in Python.", true, 1, "python", "Python" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "Build semantic web structures, responsive layouts, and 16-bit interfaces with HTML5 and CSS.", true, 2, "html-css", "HTML & CSS" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "Conquer DOM manipulation, asynchronous runes, and interactive client-side logic with JavaScript.", true, 3, "js", "JavaScript" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), "Design relational schemas, write SQL queries, and defend data fortresses with PostgreSQL.", true, 4, "postgres", "PostgreSQL" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_analytics_events_created_at",
                table: "analytics_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_analytics_events_event_type_module_slug",
                table: "analytics_events",
                columns: new[] { "event_type", "module_slug" });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_user_id",
                table: "analytics_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_learning_modules_order_index",
                table: "learning_modules",
                column: "order_index");

            migrationBuilder.CreateIndex(
                name: "ix_learning_modules_slug",
                table: "learning_modules",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_events");

            migrationBuilder.DropTable(
                name: "learning_modules");
        }
    }
}
