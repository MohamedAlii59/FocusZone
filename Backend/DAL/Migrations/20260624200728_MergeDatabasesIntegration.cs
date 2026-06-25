using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class MergeDatabasesIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_Goals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                    table.CheckConstraint("CK_Resources_Type", "Type IN ('Youtube', 'Course', 'Book', 'Article', 'PDF', 'Documentation')");
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.TopicId);
                    table.CheckConstraint("CK_Topics_Type", "Type IN ('Domain', 'Concept', 'Technique', 'Tool', 'Career')");
                });

            migrationBuilder.CreateTable(
                name: "StudySessions",
                columns: table => new
                {
                    SessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResourceId = table.Column<long>(type: "bigint", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionSummary = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_StudySessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudySessions_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceTopicCoverages",
                columns: table => new
                {
                    ResourceId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    CoverageWeight = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    DifficultyContribution = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceTopicCoverages", x => new { x.ResourceId, x.TopicId });
                    table.CheckConstraint("CK_RTC_CoverageWeight", "CoverageWeight >= 0.00 AND CoverageWeight <= 1.00");
                    table.CheckConstraint("CK_RTC_DifficultyContribution", "DifficultyContribution >= 0.00 AND DifficultyContribution <= 1.00");
                    table.ForeignKey(
                        name: "FK_ResourceTopicCoverages_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceTopicCoverages_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TopicRelationships",
                columns: table => new
                {
                    RelationshipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceTopicId = table.Column<int>(type: "int", nullable: false),
                    TargetTopicId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicRelationships", x => x.RelationshipId);
                    table.CheckConstraint("CK_TopicRel_NoSelfRef", "SourceTopicId <> TargetTopicId");
                    table.CheckConstraint("CK_TopicRel_Type", "RelationshipType IN ('contains', 'prerequisite_for', 'required_for', 'related_to')");
                    table.ForeignKey(
                        name: "FK_TopicRelationships_Topics_SourceTopicId",
                        column: x => x.SourceTopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopicRelationships_Topics_TargetTopicId",
                        column: x => x.TargetTopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDomains",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDomains", x => new { x.UserId, x.TopicId });
                    table.CheckConstraint("CK_UserDomains_Score", "Score >= 0.00 AND Score <= 1.00");
                    table.ForeignKey(
                        name: "FK_UserDomains_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDomains_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTopicMasteries",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Mastery = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Interest = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    EvidenceCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopicMasteries", x => new { x.UserId, x.TopicId });
                    table.CheckConstraint("CK_UTM_Confidence", "Confidence >= 0.00 AND Confidence <= 1.00");
                    table.CheckConstraint("CK_UTM_Interest", "Interest >= 0.00 AND Interest <= 1.00");
                    table.CheckConstraint("CK_UTM_Mastery", "Mastery >= 0.00 AND Mastery <= 1.00");
                    table.ForeignKey(
                        name: "FK_UserTopicMasteries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTopicMasteries_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evidence",
                columns: table => new
                {
                    EvidenceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.EvidenceId);
                    table.CheckConstraint("CK_Evidence_Score", "Score >= 0.00 AND Score <= 1.00");
                    table.CheckConstraint("CK_Evidence_Type", "Type IN ('quiz', 'study_time', 'assessment', 'retention_test')");
                    table.ForeignKey(
                        name: "FK_Evidence_StudySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "StudySessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evidence_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Questions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_StudySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "StudySessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_SessionId",
                table: "Evidence",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_TopicId",
                table: "Evidence",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SessionId",
                table: "Questions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UserId",
                table: "Questions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceTopicCoverages_TopicId",
                table: "ResourceTopicCoverages",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_ResourceId",
                table: "StudySessions",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_UserId",
                table: "StudySessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicRelationships_SourceTopicId",
                table: "TopicRelationships",
                column: "SourceTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicRelationships_TargetTopicId",
                table: "TopicRelationships",
                column: "TargetTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Name",
                table: "Topics",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDomains_TopicId",
                table: "UserDomains",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicMasteries_TopicId",
                table: "UserTopicMasteries",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evidence");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "ResourceTopicCoverages");

            migrationBuilder.DropTable(
                name: "TopicRelationships");

            migrationBuilder.DropTable(
                name: "UserDomains");

            migrationBuilder.DropTable(
                name: "UserTopicMasteries");

            migrationBuilder.DropTable(
                name: "StudySessions");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
