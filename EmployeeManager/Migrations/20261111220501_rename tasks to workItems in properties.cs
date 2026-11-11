using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManager.Migrations
{
    /// <inheritdoc />
    public partial class renametaskstoworkItemsinproperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Employees_EmployeeId_EmployeeUserId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskCategories_CategoryId_CategoryUserId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "TaskCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "EmployeeUserId",
                table: "WorkItems",
                newName: "AssignedToUserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "WorkItems",
                newName: "AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_EmployeeId_EmployeeUserId",
                table: "WorkItems",
                newName: "IX_WorkItems_AssignedToId_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_CategoryId_CategoryUserId",
                table: "WorkItems",
                newName: "IX_WorkItems_CategoryId_CategoryUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "WorkItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemCategories", x => new { x.Id, x.UserId });
                });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Employees_AssignedToId_AssignedToUserId",
                table: "WorkItems",
                columns: new[] { "AssignedToId", "AssignedToUserId" },
                principalTable: "Employees",
                principalColumns: new[] { "Id", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId_CategoryUserId",
                table: "WorkItems",
                columns: new[] { "CategoryId", "CategoryUserId" },
                principalTable: "WorkItemCategories",
                principalColumns: new[] { "Id", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Employees_AssignedToId_AssignedToUserId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_WorkItemCategories_CategoryId_CategoryUserId",
                table: "WorkItems");

            migrationBuilder.DropTable(
                name: "WorkItemCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems");

            migrationBuilder.RenameTable(
                name: "WorkItems",
                newName: "Tasks");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "Tasks",
                newName: "EmployeeUserId");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "Tasks",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_CategoryId_CategoryUserId",
                table: "Tasks",
                newName: "IX_Tasks_CategoryId_CategoryUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_AssignedToId_AssignedToUserId",
                table: "Tasks",
                newName: "IX_Tasks_EmployeeId_EmployeeUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TaskCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategories", x => new { x.Id, x.UserId });
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Employees_EmployeeId_EmployeeUserId",
                table: "Tasks",
                columns: new[] { "EmployeeId", "EmployeeUserId" },
                principalTable: "Employees",
                principalColumns: new[] { "Id", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskCategories_CategoryId_CategoryUserId",
                table: "Tasks",
                columns: new[] { "CategoryId", "CategoryUserId" },
                principalTable: "TaskCategories",
                principalColumns: new[] { "Id", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
