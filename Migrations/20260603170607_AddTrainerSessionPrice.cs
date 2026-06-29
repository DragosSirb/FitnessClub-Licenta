using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerSessionPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SessionPrice",
                table: "Trainers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionPrice",
                table: "Trainers");
        }
    }
}
