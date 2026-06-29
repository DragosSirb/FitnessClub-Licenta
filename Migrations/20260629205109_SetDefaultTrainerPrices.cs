using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultTrainerPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Trainers SET SessionPrice = 100 WHERE SessionPrice = 0 OR SessionPrice IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
