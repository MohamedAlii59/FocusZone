using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations.ExamSessionChanges
{
    /// <inheritdoc />
    public partial class RenameExamSessionKeyAndAddFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionAnswers_ExamSessions_SessionId",
                table: "SessionAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamSessions",
                table: "ExamSessions");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "SessionAnswers",
                newName: "ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_SessionAnswers_SessionId",
                table: "SessionAnswers",
                newName: "IX_SessionAnswers_ExamId");

            // Add new primary key column 'ExamId' (non-identity to avoid conflict)
            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "ExamSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Add new FK column that will reference StudySessions (copy of old SessionId values)
            migrationBuilder.AddColumn<long>(
                name: "StudySessionSessionId",
                table: "ExamSessions",
                type: "bigint",
                nullable: true);

            // Copy existing SessionId values into StudySessionSessionId
            migrationBuilder.Sql(@"UPDATE ExamSessions SET StudySessionSessionId = SessionId");

            // Drop the old SessionId column (which was PK and identity)
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "ExamSessions");

            // Make StudySessionSessionId non-nullable
            migrationBuilder.AlterColumn<long>(
                name: "StudySessionSessionId",
                table: "ExamSessions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamSessions",
                table: "ExamSessions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_StudySessionSessionId",
                table: "ExamSessions",
                column: "StudySessionSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSessions_StudySessions_StudySessionSessionId",
                table: "ExamSessions",
                column: "StudySessionSessionId",
                principalTable: "StudySessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionAnswers_ExamSessions_ExamId",
                table: "SessionAnswers",
                column: "ExamId",
                principalTable: "ExamSessions",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSessions_StudySessions_StudySessionSessionId",
                table: "ExamSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionAnswers_ExamSessions_ExamId",
                table: "SessionAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamSessions",
                table: "ExamSessions");

            migrationBuilder.DropIndex(
                name: "IX_ExamSessions_StudySessionSessionId",
                table: "ExamSessions");

            // Recreate old SessionId column as int identity
            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "ExamSessions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Copy back StudySessionSessionId into SessionId where possible (best effort)
            // Note: data loss may occur if values exceed int range
            migrationBuilder.Sql(@"UPDATE ExamSessions SET SessionId = CAST(StudySessionSessionId AS int)");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "ExamSessions");

            migrationBuilder.DropColumn(
                name: "StudySessionSessionId",
                table: "ExamSessions");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "SessionAnswers",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_SessionAnswers_ExamId",
                table: "SessionAnswers",
                newName: "IX_SessionAnswers_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamSessions",
                table: "ExamSessions",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionAnswers_ExamSessions_SessionId",
                table: "SessionAnswers",
                column: "SessionId",
                principalTable: "ExamSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
