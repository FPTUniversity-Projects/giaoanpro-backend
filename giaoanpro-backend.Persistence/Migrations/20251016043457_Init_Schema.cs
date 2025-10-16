using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giaoanpro_backend.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class Init_Schema : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Attributes",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Attributes", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Grades",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Level = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Grades", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "QuestionBanks",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_QuestionBanks", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Semesters",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Semesters", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "SubscriptionPlans",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
					DurationInDays = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
					PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
					FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
					IsActive = table.Column<bool>(type: "bit", nullable: false),
					Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Subjects",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Subjects", x => x.Id);
					table.ForeignKey(
						name: "FK_Subjects_Grades_GradeId",
						column: x => x.GradeId,
						principalTable: "Grades",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Classes",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Classes", x => x.Id);
					table.ForeignKey(
						name: "FK_Classes_Grades_GradeId",
						column: x => x.GradeId,
						principalTable: "Grades",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Classes_Semesters_SemesterId",
						column: x => x.SemesterId,
						principalTable: "Semesters",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Classes_Users_TeacherId",
						column: x => x.TeacherId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "PromptLogs",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PromptLogs", x => x.Id);
					table.ForeignKey(
						name: "FK_PromptLogs_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Subscriptions",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Subscriptions", x => x.Id);
					table.ForeignKey(
						name: "FK_Subscriptions_SubscriptionPlans_PlanId",
						column: x => x.PlanId,
						principalTable: "SubscriptionPlans",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Subscriptions_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "LessonPlans",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Objective = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LessonPlans", x => x.Id);
					table.ForeignKey(
						name: "FK_LessonPlans_Subjects_SubjectId",
						column: x => x.SubjectId,
						principalTable: "Subjects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_LessonPlans_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Syllabuses",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Syllabuses", x => x.Id);
					table.ForeignKey(
						name: "FK_Syllabuses_Subjects_SubjectId",
						column: x => x.SubjectId,
						principalTable: "Subjects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "ClassMembers",
				columns: table => new
				{
					StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ClassMembers", x => new { x.StudentId, x.ClassId });
					table.ForeignKey(
						name: "FK_ClassMembers_Classes_ClassId",
						column: x => x.ClassId,
						principalTable: "Classes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ClassMembers_Users_StudentId",
						column: x => x.StudentId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Questions",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					BankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					PromptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Questions", x => x.Id);
					table.ForeignKey(
						name: "FK_Questions_PromptLogs_PromptId",
						column: x => x.PromptId,
						principalTable: "PromptLogs",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Questions_QuestionBanks_BankId",
						column: x => x.BankId,
						principalTable: "QuestionBanks",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Activitys",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					LessonPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Objective = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Product = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Implementation = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Activitys", x => x.Id);
					table.ForeignKey(
						name: "FK_Activitys_LessonPlans_LessonPlanId",
						column: x => x.LessonPlanId,
						principalTable: "LessonPlans",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ExamMatrices",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					LessonPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
					NumberOfQuestions = table.Column<int>(type: "int", nullable: false),
					MarksPerQuestion = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ExamMatrices", x => x.Id);
					table.ForeignKey(
						name: "FK_ExamMatrices_LessonPlans_LessonPlanId",
						column: x => x.LessonPlanId,
						principalTable: "LessonPlans",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_ExamMatrices_Subjects_SubjectId",
						column: x => x.SubjectId,
						principalTable: "Subjects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "QuestionAttributes",
				columns: table => new
				{
					QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_QuestionAttributes", x => new { x.QuestionId, x.AttributeId });
					table.ForeignKey(
						name: "FK_QuestionAttributes_Attributes_AttributeId",
						column: x => x.AttributeId,
						principalTable: "Attributes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_QuestionAttributes_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "QuestionOptions",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
					IsCorrect = table.Column<bool>(type: "bit", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_QuestionOptions", x => x.Id);
					table.ForeignKey(
						name: "FK_QuestionOptions_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Exams",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					MatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					DurationMinutes = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Exams", x => x.Id);
					table.ForeignKey(
						name: "FK_Exams_Activitys_ActivityId",
						column: x => x.ActivityId,
						principalTable: "Activitys",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Exams_ExamMatrices_MatrixId",
						column: x => x.MatrixId,
						principalTable: "ExamMatrices",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Exams_Users_CreatorId",
						column: x => x.CreatorId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Attempts",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					FinalScore = table.Column<int>(type: "int", nullable: false),
					Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Attempts", x => x.Id);
					table.ForeignKey(
						name: "FK_Attempts_Exams_ExamId",
						column: x => x.ExamId,
						principalTable: "Exams",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Attempts_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "ExamQuestions",
				columns: table => new
				{
					ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					SequenceNumber = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ExamQuestions", x => new { x.ExamId, x.QuestionId });
					table.ForeignKey(
						name: "FK_ExamQuestions_Exams_ExamId",
						column: x => x.ExamId,
						principalTable: "Exams",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ExamQuestions_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AttemptDetails",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
					IsCorrect = table.Column<bool>(type: "bit", nullable: false),
					Score = table.Column<int>(type: "int", nullable: false),
					Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AttemptDetails", x => x.Id);
					table.ForeignKey(
						name: "FK_AttemptDetails_Attempts_AttemptId",
						column: x => x.AttemptId,
						principalTable: "Attempts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AttemptDetails_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Activitys_LessonPlanId",
				table: "Activitys",
				column: "LessonPlanId");

			migrationBuilder.CreateIndex(
				name: "IX_AttemptDetails_AttemptId",
				table: "AttemptDetails",
				column: "AttemptId");

			migrationBuilder.CreateIndex(
				name: "IX_AttemptDetails_QuestionId",
				table: "AttemptDetails",
				column: "QuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_Attempts_ExamId",
				table: "Attempts",
				column: "ExamId");

			migrationBuilder.CreateIndex(
				name: "IX_Attempts_UserId",
				table: "Attempts",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_Classes_GradeId",
				table: "Classes",
				column: "GradeId");

			migrationBuilder.CreateIndex(
				name: "IX_Classes_SemesterId",
				table: "Classes",
				column: "SemesterId");

			migrationBuilder.CreateIndex(
				name: "IX_Classes_TeacherId",
				table: "Classes",
				column: "TeacherId");

			migrationBuilder.CreateIndex(
				name: "IX_ClassMembers_ClassId",
				table: "ClassMembers",
				column: "ClassId");

			migrationBuilder.CreateIndex(
				name: "IX_ExamMatrices_LessonPlanId",
				table: "ExamMatrices",
				column: "LessonPlanId");

			migrationBuilder.CreateIndex(
				name: "IX_ExamMatrices_SubjectId",
				table: "ExamMatrices",
				column: "SubjectId");

			migrationBuilder.CreateIndex(
				name: "IX_ExamQuestions_QuestionId",
				table: "ExamQuestions",
				column: "QuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_Exams_ActivityId",
				table: "Exams",
				column: "ActivityId");

			migrationBuilder.CreateIndex(
				name: "IX_Exams_CreatorId",
				table: "Exams",
				column: "CreatorId");

			migrationBuilder.CreateIndex(
				name: "IX_Exams_MatrixId",
				table: "Exams",
				column: "MatrixId");

			migrationBuilder.CreateIndex(
				name: "IX_LessonPlans_SubjectId",
				table: "LessonPlans",
				column: "SubjectId");

			migrationBuilder.CreateIndex(
				name: "IX_LessonPlans_UserId",
				table: "LessonPlans",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_PromptLogs_UserId",
				table: "PromptLogs",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_QuestionAttributes_AttributeId",
				table: "QuestionAttributes",
				column: "AttributeId");

			migrationBuilder.CreateIndex(
				name: "IX_QuestionOptions_QuestionId",
				table: "QuestionOptions",
				column: "QuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_Questions_BankId",
				table: "Questions",
				column: "BankId");

			migrationBuilder.CreateIndex(
				name: "IX_Questions_PromptId",
				table: "Questions",
				column: "PromptId");

			migrationBuilder.CreateIndex(
				name: "IX_Subjects_GradeId",
				table: "Subjects",
				column: "GradeId");

			migrationBuilder.CreateIndex(
				name: "IX_Subscriptions_PlanId",
				table: "Subscriptions",
				column: "PlanId");

			migrationBuilder.CreateIndex(
				name: "IX_Subscriptions_UserId",
				table: "Subscriptions",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_Syllabuses_SubjectId",
				table: "Syllabuses",
				column: "SubjectId",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AttemptDetails");

			migrationBuilder.DropTable(
				name: "ClassMembers");

			migrationBuilder.DropTable(
				name: "ExamQuestions");

			migrationBuilder.DropTable(
				name: "QuestionAttributes");

			migrationBuilder.DropTable(
				name: "QuestionOptions");

			migrationBuilder.DropTable(
				name: "Subscriptions");

			migrationBuilder.DropTable(
				name: "Syllabuses");

			migrationBuilder.DropTable(
				name: "Attempts");

			migrationBuilder.DropTable(
				name: "Classes");

			migrationBuilder.DropTable(
				name: "Attributes");

			migrationBuilder.DropTable(
				name: "Questions");

			migrationBuilder.DropTable(
				name: "SubscriptionPlans");

			migrationBuilder.DropTable(
				name: "Exams");

			migrationBuilder.DropTable(
				name: "Semesters");

			migrationBuilder.DropTable(
				name: "PromptLogs");

			migrationBuilder.DropTable(
				name: "QuestionBanks");

			migrationBuilder.DropTable(
				name: "Activitys");

			migrationBuilder.DropTable(
				name: "ExamMatrices");

			migrationBuilder.DropTable(
				name: "LessonPlans");

			migrationBuilder.DropTable(
				name: "Subjects");

			migrationBuilder.DropTable(
				name: "Users");

			migrationBuilder.DropTable(
				name: "Grades");
		}
	}
}
