using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Attribute = giaoanpro_backend.Domain.Entities.Attribute;

namespace giaoanpro_backend.Persistence.Context
{
	public partial class GiaoanproDBContext : DbContext
	{
		public GiaoanproDBContext() { }
		public GiaoanproDBContext(DbContextOptions<GiaoanproDBContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
		public DbSet<Syllabus> Syllabuses { get; set; }
		public DbSet<Subject> Subjects { get; set; }
		public DbSet<Subscription> Subscriptions { get; set; }
		public DbSet<LessonPlan> LessonPlans { get; set; }
		public DbSet<Grade> Grades { get; set; }
		public DbSet<Class> Classes { get; set; }
		public DbSet<Semester> Semesters { get; set; }
		public DbSet<Question> Questions { get; set; }
		public DbSet<Exam> Exams { get; set; }
		public DbSet<ExamQuestion> ExamQuestions { get; set; }
		public DbSet<ExamMatrix> ExamMatrices { get; set; }
		public DbSet<ExamMatrixDetail> ExamMatrixDetails { get; set; }
		public DbSet<Attempt> Attempts { get; set; }
		public DbSet<AttemptDetail> AttemptDetails { get; set; }
		public DbSet<QuestionAttribute> QuestionAttributes { get; set; }
		public DbSet<Attribute> Attributes { get; set; }
		public DbSet<PromptLog> PromptLogs { get; set; }
		public DbSet<QuestionOption> QuestionOptions { get; set; }
		public DbSet<Activity> Activitys { get; set; }
		public DbSet<ClassMember> ClassMembers { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Material> Materials { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Apply global query filter for soft-delete entities (DeletedAt == null)
			var softDeleteInterface = typeof(ISoftDeleteEntity);
			var entityTypes = modelBuilder.Model.GetEntityTypes()
				.Where(t => !t.IsOwned() && t.ClrType != null && softDeleteInterface.IsAssignableFrom(t.ClrType))
				.ToList();

			foreach (var et in entityTypes)
			{
				var clrType = et.ClrType!;
				// Build lambda: (e) => EF.Property<DateTime?>(e, "DeletedAt") == null
				var parameter = Expression.Parameter(clrType, "e");
				var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property), new[] { typeof(object), typeof(string) })!
					.MakeGenericMethod(typeof(DateTime?));
				var deletedAtProperty = Expression.Call(efPropertyMethod, parameter, Expression.Constant(nameof(ISoftDeleteEntity.DeletedAt)));
				var compareNull = Expression.Equal(deletedAtProperty, Expression.Constant(null, typeof(DateTime?)));
				var lambda = Expression.Lambda(compareNull, parameter);

				modelBuilder.Entity(clrType).HasQueryFilter(lambda);
			}

			// Composite keys for join entities
			modelBuilder.Entity<ClassMember>()
				.HasKey(cm => new { cm.StudentId, cm.ClassId });

			modelBuilder.Entity<ExamQuestion>()
				.HasKey(eq => new { eq.ExamId, eq.QuestionId });

			modelBuilder.Entity<QuestionAttribute>()
				.HasKey(qa => new { qa.QuestionId, qa.AttributeId });

			// ClassMember relationships
			modelBuilder.Entity<ClassMember>()
				.HasOne(cm => cm.Student)
				.WithMany(u => u.ClassMemberships)
				.HasForeignKey(cm => cm.StudentId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ClassMember>()
				.HasOne(cm => cm.Class)
				.WithMany(c => c.Members)
				.HasForeignKey(cm => cm.ClassId)
				.OnDelete(DeleteBehavior.Cascade);

			// Class -> Teacher
			modelBuilder.Entity<Class>()
				.HasOne(c => c.Teacher)
				.WithMany(u => u.ClassesTaught)
				.HasForeignKey(c => c.TeacherId)
				.OnDelete(DeleteBehavior.Restrict);

			// Class -> Grade & Semester
			modelBuilder.Entity<Class>()
				.HasOne(c => c.Grade)
				.WithMany(g => g.Classes)
				.HasForeignKey(c => c.GradeId)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Class>()
				.HasOne(c => c.Semester)
				.WithMany(s => s.Classes)
				.HasForeignKey(c => c.SemesterId)
				.OnDelete(DeleteBehavior.Restrict);

			// ExamQuestion relationships (use explicit collections)
			modelBuilder.Entity<ExamQuestion>()
				.HasOne(eq => eq.Exam)
				.WithMany(e => e.ExamQuestions)
				.HasForeignKey(eq => eq.ExamId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ExamQuestion>()
				.HasOne(eq => eq.Question)
				.WithMany(q => q.ExamQuestions)
				.HasForeignKey(eq => eq.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			// QuestionAttribute relationships (Question side no longer has navigation)
			modelBuilder.Entity<QuestionAttribute>()
				.HasOne(qa => qa.Question)
				.WithMany()
				.HasForeignKey(qa => qa.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<QuestionAttribute>()
				.HasOne(qa => qa.Attribute)
				.WithMany(a => a.QuestionAttributes)
				.HasForeignKey(qa => qa.AttributeId)
				.OnDelete(DeleteBehavior.Cascade);

			// Attempt -> AttemptDetails
			modelBuilder.Entity<Attempt>()
				.HasMany(a => a.AttemptDetails)
				.WithOne(ad => ad.Attempt)
				.HasForeignKey(ad => ad.AttemptId)
				.OnDelete(DeleteBehavior.Cascade);

			// Attempt -> User & Exam
			modelBuilder.Entity<Attempt>()
				.HasOne(a => a.User)
				.WithMany(u => u.Attempts)
				.HasForeignKey(a => a.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Attempt>()
				.HasOne(a => a.Exam)
				.WithMany()
				.HasForeignKey(a => a.ExamId)
				.OnDelete(DeleteBehavior.Restrict);

			// Question -> Options
			modelBuilder.Entity<QuestionOption>()
				.HasOne(o => o.Question)
				.WithMany(q => q.Options)
				.HasForeignKey(o => o.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Question -> AttemptDetails
			modelBuilder.Entity<Question>()
				.HasMany(q => q.AttemptDetails)
				.WithOne(ad => ad.Question)
				.HasForeignKey(ad => ad.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			// PromptLog -> User
			modelBuilder.Entity<PromptLog>()
				.HasOne(p => p.User)
				.WithMany(u => u.PromptLogs)
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Subscription -> User & Plan
			modelBuilder.Entity<Subscription>()
				.HasOne(s => s.User)
				.WithMany(u => u.Subscriptions)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Subscription>()
				.HasOne(s => s.Plan)
				.WithMany(p => p.UserSubscriptions)
				.HasForeignKey(s => s.PlanId)
				.OnDelete(DeleteBehavior.Restrict);

			// Payment -> Subscription
			modelBuilder.Entity<Payment>()
				.HasOne(p => p.Subscription)
				.WithMany(s => s.Payments)
				.HasForeignKey(p => p.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Exam -> Matrix, Creator and activity wiring
			modelBuilder.Entity<Exam>()
				.HasOne(e => e.Matrix)
				.WithMany(em => em.Exams)
				.HasForeignKey(e => e.MatrixId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Exam>()
				.HasOne(e => e.Creator)
				.WithMany(u => u.CreatedExams)
				.HasForeignKey(e => e.CreatorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Exam>()
				.HasOne(e => e.Activity)
				.WithMany(u => u.Exams)
				.HasForeignKey(e => e.ActivityId)
				.OnDelete(DeleteBehavior.Restrict);

			// ExamMatrix -> Subject & LessonPlan
			modelBuilder.Entity<ExamMatrix>()
				.HasOne(em => em.Subject)
				.WithMany(u => u.ExamMatrices)
				.HasForeignKey(em => em.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure ExamMatrixDetail entity
			modelBuilder.Entity<ExamMatrixDetail>()
				.HasKey(ml => ml.Id);
			modelBuilder.Entity<ExamMatrixDetail>()
				.HasIndex("MatrixId");
			modelBuilder.Entity<ExamMatrixDetail>()
				.HasOne(ml => ml.Matrix)
				.WithMany(m => m.Lines)
				.HasForeignKey(ml => ml.MatrixId)
				.OnDelete(DeleteBehavior.Cascade);

			// Activity -> LessonPlan
			modelBuilder.Entity<Activity>()
				.HasOne(a => a.LessonPlan)
				.WithMany(lp => lp.Activities)
				.HasForeignKey(a => a.LessonPlanId)
				.OnDelete(DeleteBehavior.Cascade);

			// Activity parent/child relationship
			modelBuilder.Entity<Activity>()
				.HasOne(a => a.Parent)
				.WithMany(p => p.Children) // Use the new Children navigation property
				.HasForeignKey(a => a.ParentId)
				.IsRequired(false) // ParentId is nullable
				.OnDelete(DeleteBehavior.Restrict);

			// Material -> Activity
			modelBuilder.Entity<Material>()
				.HasOne(m => m.Activity)
				.WithMany(a => a.Materials)
				.HasForeignKey(m => m.ActivityId)
				.OnDelete(DeleteBehavior.Cascade);

			// LessonPlan -> User & Subject
			modelBuilder.Entity<LessonPlan>()
				.HasOne(lp => lp.User)
				.WithMany(u => u.LessonPlans)
				.HasForeignKey(lp => lp.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<LessonPlan>()
				.HasOne(lp => lp.Subject)
				.WithMany(s => s.LessonPlans)
				.HasForeignKey(lp => lp.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			// LessonPlan -> Questions (1:n)
			modelBuilder.Entity<Question>()
				.HasOne(q => q.LessonPlan)
				.WithMany(lp => lp.Questions)
				.HasForeignKey(q => q.LessonPlanId)
				.OnDelete(DeleteBehavior.Cascade);

			// Removed Question -> Prompt mapping (Question no longer references Prompt)

			// Subject -> Grade & Syllabus
			modelBuilder.Entity<Subject>()
				.HasOne(q => q.Grade)
				.WithMany(g => g.Subjects)
				.HasForeignKey(q => q.GradeId)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Subject>()
				.HasOne(q => q.Syllabus)
				.WithOne(s => s.Subject)
				.HasForeignKey<Syllabus>(s => s.SubjectId)
				.OnDelete(DeleteBehavior.Restrict);

			// SubscriptionPlan: set precision for decimal Price to avoid truncation warnings
			modelBuilder.Entity<SubscriptionPlan>()
				.Property(sp => sp.Price)
				.HasPrecision(18, 2);

			// Payment: set precision for decimal AmountPaid to avoid truncation warnings
			modelBuilder.Entity<Payment>()
				.Property(p => p.AmountPaid)
				.HasPrecision(18, 2);


			// Enums stored as strings
			modelBuilder.Entity<Attempt>()
				.Property(a => a.Status)
				.HasConversion<string>();
			modelBuilder.Entity<Question>()
				.Property(q => q.QuestionType)
				.HasConversion<string>();
			modelBuilder.Entity<Question>()
				.Property(q => q.DifficultyLevel)
				.HasConversion<string>();
			modelBuilder.Entity<Question>()
				.Property(q => q.AwarenessLevel)
				.HasConversion<string>();
			modelBuilder.Entity<Attribute>()
				.Property(a => a.Value)
				.HasConversion<string>();
			modelBuilder.Entity<Activity>()
				.Property(a => a.Type)
				.HasConversion<string>();
			modelBuilder.Entity<Material>()
				.Property(m => m.Type)
				.HasConversion<string>();
			modelBuilder.Entity<Subscription>()
				.Property(s => s.Status)
				.HasConversion<string>();
			modelBuilder.Entity<User>()
				.Property(u => u.Role)
				.HasConversion<string>();
			modelBuilder.Entity<Payment>()
				.Property(p => p.Status)
				.HasConversion<string>();

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

		public override int SaveChanges()
		{
			UpdateAuditAndSoftDeleteFields();
			return base.SaveChanges();
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			UpdateAuditAndSoftDeleteFields();
			return await base.SaveChangesAsync(cancellationToken);
		}

		private void UpdateAuditAndSoftDeleteFields()
		{
			var now = DateTime.UtcNow;

			foreach (var entry in ChangeTracker.Entries())
			{
				if (entry.Entity is ISoftDeleteEntity softDeleteEntity)
				{
					if (entry.State == EntityState.Deleted)
					{
						entry.State = EntityState.Modified;
						softDeleteEntity.DeletedAt = now;
					}
					else if (entry.State == EntityState.Added)
					{
						softDeleteEntity.DeletedAt = null;
					}
				}

				if (entry.Entity is AuditableEntity auditableEntity)
				{
					if (entry.State == EntityState.Added)
					{
						auditableEntity.CreatedAt = now;
						auditableEntity.UpdatedAt = now;
					}
					else if (entry.State == EntityState.Modified)
					{
						auditableEntity.UpdatedAt = now;
					}
				}
			}
		}
	}
}
