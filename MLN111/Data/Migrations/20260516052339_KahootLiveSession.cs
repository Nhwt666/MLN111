using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MLN111.Data.Migrations
{
    /// <inheritdoc />
    public partial class KahootLiveSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "quiz_rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    JoinCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SecondsPerQuestion = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    CurrentQuestionIndex = table.Column<int>(type: "integer", nullable: true),
                    QuestionStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SessionStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SessionFinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_rooms_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quiz_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_participants_quiz_rooms_QuizRoomId",
                        column: x => x.QuizRoomId,
                        principalTable: "quiz_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_participants_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_questions_quiz_rooms_QuizRoomId",
                        column: x => x.QuizRoomId,
                        principalTable: "quiz_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_choices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_choices_quiz_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "quiz_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_choices_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "quiz_choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "quiz_participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "quiz_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_ChoiceId",
                table: "quiz_answers",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_ParticipantId_QuestionId",
                table: "quiz_answers",
                columns: new[] { "ParticipantId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_QuestionId",
                table: "quiz_answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_choices_QuestionId",
                table: "quiz_choices",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_participants_QuizRoomId_TotalScore",
                table: "quiz_participants",
                columns: new[] { "QuizRoomId", "TotalScore" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_participants_QuizRoomId_UserId",
                table: "quiz_participants",
                columns: new[] { "QuizRoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_participants_UserId",
                table: "quiz_participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_QuizRoomId_OrderIndex",
                table: "quiz_questions",
                columns: new[] { "QuizRoomId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_rooms_CreatedById",
                table: "quiz_rooms",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_rooms_JoinCode",
                table: "quiz_rooms",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_choices");

            migrationBuilder.DropTable(
                name: "quiz_participants");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_rooms");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "users");
        }
    }
}
