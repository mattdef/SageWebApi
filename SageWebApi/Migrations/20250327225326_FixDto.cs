using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SageWebApi.Migrations
{
    /// <inheritdoc />
    public partial class FixDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "EcritureChangeDtos");

            migrationBuilder.RenameColumn(
                name: "NumPiece",
                table: "EcritureChangeDtos",
                newName: "NumEcriture");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumEcriture",
                table: "EcritureChangeDtos",
                newName: "NumPiece");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "EcritureChangeDtos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
