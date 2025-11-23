using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class otp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewPasswordVM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewPasswordVM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidateOTP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidateOTP", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewPasswordVM");

            migrationBuilder.DropTable(
                name: "ValidateOTP");
        }
    }
}
