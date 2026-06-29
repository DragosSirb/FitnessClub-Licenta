using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.Migrations
{
    /// <inheritdoc />
    public partial class TranslateQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Text",
                value: "Singurul antrenament prost este cel care nu a avut loc.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Text",
                value: "Progres, nu perfecțiune.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Text",
                value: "Împinge-te tu însuți, pentru că nimeni altcineva nu o va face pentru tine.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Text",
                value: "Succesul începe cu autodisciplina.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Text",
                value: "Corpul tău poate suporta aproape orice. Mintea ta este cea pe care trebuie să o convingi.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Text",
                value: "The only bad workout is the one that didn't happen.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Text",
                value: "Progress, not perfection.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Text",
                value: "Push yourself because no one else is going to do it for you.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Text",
                value: "Success starts with self-discipline.");

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Text",
                value: "Your body can stand almost anything. It's your mind you have to convince.");
        }
    }
}
