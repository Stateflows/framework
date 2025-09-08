using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarszawskieDniInformatyki.Migrations
{
    /// <inheritdoc />
    public partial class Values : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateflowsValue_v1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BehaviorType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BehaviorName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BehaviorInstance = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateflowsValue_v1", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsValue_v1_BehaviorInstance",
                table: "StateflowsValue_v1",
                column: "BehaviorInstance");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsValue_v1_BehaviorName",
                table: "StateflowsValue_v1",
                column: "BehaviorName");

            migrationBuilder.CreateIndex(
                name: "IX_StateflowsValue_v1_BehaviorType",
                table: "StateflowsValue_v1",
                column: "BehaviorType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateflowsValue_v1");
        }
    }
}
