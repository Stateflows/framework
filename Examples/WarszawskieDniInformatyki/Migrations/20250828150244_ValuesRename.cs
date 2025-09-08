using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarszawskieDniInformatyki.Migrations
{
    /// <inheritdoc />
    public partial class ValuesRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StateflowsValue_v1",
                table: "StateflowsValue_v1");

            migrationBuilder.RenameTable(
                name: "StateflowsValue_v1",
                newName: "StateflowsValues_v1");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValue_v1_BehaviorType",
                table: "StateflowsValues_v1",
                newName: "IX_StateflowsValues_v1_BehaviorType");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValue_v1_BehaviorName",
                table: "StateflowsValues_v1",
                newName: "IX_StateflowsValues_v1_BehaviorName");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValue_v1_BehaviorInstance",
                table: "StateflowsValues_v1",
                newName: "IX_StateflowsValues_v1_BehaviorInstance");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateflowsValues_v1",
                table: "StateflowsValues_v1",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StateflowsValues_v1",
                table: "StateflowsValues_v1");

            migrationBuilder.RenameTable(
                name: "StateflowsValues_v1",
                newName: "StateflowsValue_v1");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValues_v1_BehaviorType",
                table: "StateflowsValue_v1",
                newName: "IX_StateflowsValue_v1_BehaviorType");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValues_v1_BehaviorName",
                table: "StateflowsValue_v1",
                newName: "IX_StateflowsValue_v1_BehaviorName");

            migrationBuilder.RenameIndex(
                name: "IX_StateflowsValues_v1_BehaviorInstance",
                table: "StateflowsValue_v1",
                newName: "IX_StateflowsValue_v1_BehaviorInstance");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StateflowsValue_v1",
                table: "StateflowsValue_v1",
                column: "Id");
        }
    }
}
