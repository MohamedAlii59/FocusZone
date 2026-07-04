using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionExamActiveAndWhiteList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionExamActives",
                columns: table => new
                {
                    SessionExamActiveId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionExamActives", x => x.SessionExamActiveId);
                    table.ForeignKey(
                        name: "FK_SessionExamActives_StudySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "StudySessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionWhiteLists",
                columns: table => new
                {
                    SessionWhiteListId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionWhiteLists", x => x.SessionWhiteListId);
                    table.ForeignKey(
                        name: "FK_SessionWhiteLists_StudySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "StudySessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionExamActives_SessionId",
                table: "SessionExamActives",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionWhiteLists_SessionId",
                table: "SessionWhiteLists",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionExamActives");

            migrationBuilder.DropTable(
                name: "SessionWhiteLists");
        }
    }
}
