using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarszawskieDniInformatyki.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateflowsContexts_v1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BehaviorClass = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BehaviorId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    TriggerTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggerOnStartup = table.Column<bool>(type: "bit", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateflowsContexts_v1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateflowsNotifications_v1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenderInstance = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeToLive = table.Column<int>(type: "int", nullable: false),
                    Retained = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateflowsNotifications_v1", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsContexts_v1_BehaviorClass",
                table: "StateflowsContexts_v1",
                column: "BehaviorClass");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsContexts_v1_BehaviorId",
                table: "StateflowsContexts_v1",
                column: "BehaviorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsContexts_v1_TriggerOnStartup",
                table: "StateflowsContexts_v1",
                column: "TriggerOnStartup");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsContexts_v1_TriggerTime",
                table: "StateflowsContexts_v1",
                column: "TriggerTime");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsNotifications_v1_SenderInstance",
                table: "StateflowsNotifications_v1",
                column: "SenderInstance");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsNotifications_v1_SenderName",
                table: "StateflowsNotifications_v1",
                column: "SenderName");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsNotifications_v1_SenderType",
                table: "StateflowsNotifications_v1",
                column: "SenderType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateflowsContexts_v1");

            migrationBuilder.DropTable(
                name: "StateflowsNotifications_v1");
        }
    }
}
