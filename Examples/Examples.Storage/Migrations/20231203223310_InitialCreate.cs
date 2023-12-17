using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examples.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateflowsContexts_v1", x => x.Id);
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
                name: "IX_StateflowsContexts_v1_TriggerTime",
                table: "StateflowsContexts_v1",
                column: "TriggerTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateflowsContexts_v1");
        }
    }
}
