using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SageWebApi.Migrations
{
    /// <inheritdoc />
    public partial class FixNumTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumPiece",
                table: "TiersChangeDtos",
                newName: "NumTiers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumTiers",
                table: "TiersChangeDtos",
                newName: "NumPiece");
        }
    }
}
