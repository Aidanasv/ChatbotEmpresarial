using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Version6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tone",
                table: "ChatbotSettings",
                newName: "ChatbotTone");

            migrationBuilder.RenameColumn(
                name: "BotName",
                table: "ChatbotSettings",
                newName: "ChatbotName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatbotTone",
                table: "ChatbotSettings",
                newName: "Tone");

            migrationBuilder.RenameColumn(
                name: "ChatbotName",
                table: "ChatbotSettings",
                newName: "BotName");
        }
    }
}
