using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examples.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggerOnStartup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateflowsTraces_v1");

            migrationBuilder.AddColumn<bool>(
                name: "TriggerOnStartup",
                table: "StateflowsContexts_v1",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsContexts_v1_TriggerOnStartup",
                table: "StateflowsContexts_v1",
                column: "TriggerOnStartup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StateflowsContexts_v1_TriggerOnStartup",
                table: "StateflowsContexts_v1");

            migrationBuilder.DropColumn(
                name: "TriggerOnStartup",
                table: "StateflowsContexts_v1");

            migrationBuilder.CreateTable(
                name: "StateflowsTraces_v1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BehaviorId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateflowsTraces_v1", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsTraces_v1_BehaviorId",
                table: "StateflowsTraces_v1",
                column: "BehaviorId");
        }
    }
}
