using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promising_Generation_Bank_API.Migrations
{
    /// <inheritdoc />
    public partial class AddChildIDToSavingTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildId",
                table: "SavingsTransactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildId",
                table: "SavingsTransactions");
        }
    }
}
