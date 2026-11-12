using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManager.Migrations
{
    /// <inheritdoc />
    public partial class addingremainingendpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "WorkItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WorkItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId",
                table: "WorkItems",
                column: "CategoryId",
                principalTable: "WorkItemCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WorkItems");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "WorkItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId",
                table: "WorkItems",
                column: "CategoryId",
                principalTable: "WorkItemCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
