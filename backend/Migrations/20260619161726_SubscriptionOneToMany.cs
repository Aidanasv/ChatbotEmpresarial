using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MySQL requires dropping the FK before dropping the index that backs it
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Subscriptions_SubscriptionId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_SubscriptionId",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SubscriptionId",
                table: "Companies",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Subscriptions_SubscriptionId",
                table: "Companies",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_SubscriptionId",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SubscriptionId",
                table: "Companies",
                column: "SubscriptionId",
                unique: true);
        }
    }
}
