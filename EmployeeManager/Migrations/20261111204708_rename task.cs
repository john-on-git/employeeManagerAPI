using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManager.Migrations
{
    /// <inheritdoc />
    public partial class renametask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeNotes",
                table: "Employees",
                newName: "Notes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Employees",
                newName: "EmployeeNotes");
        }
    }
}
