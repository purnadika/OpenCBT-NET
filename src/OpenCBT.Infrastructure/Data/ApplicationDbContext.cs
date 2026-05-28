using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<ExamSession> ExamSessions { get; set; }
    public DbSet<StudentResponse> StudentResponses { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<ClassRoom> ClassRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Key);
        });

        builder.Entity<Exam>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Version).IsRowVersion();
            }
            else
            {
                entity.Property(e => e.Version).IsConcurrencyToken();
            }
            
            entity.HasMany(e => e.Questions)
                  .WithOne(q => q.Exam)
                  .HasForeignKey(q => q.ExamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Text).IsRequired();
            
            entity.HasMany(q => q.Options)
                  .WithOne(o => o.Question)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AnswerOption>(entity =>
        {
            entity.Property(o => o.Text).IsRequired();
        });

        builder.Entity<ExamSession>(entity =>
        {
            entity.HasOne(es => es.Exam)
                  .WithMany()
                  .HasForeignKey(es => es.ExamId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(es => es.User)
                  .WithMany()
                  .HasForeignKey(es => es.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(es => es.Responses)
                  .WithOne(sr => sr.ExamSession)
                  .HasForeignKey(sr => sr.ExamSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentResponse>(entity =>
        {
            entity.HasOne(sr => sr.Question)
                  .WithMany()
                  .HasForeignKey(sr => sr.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sr => sr.SelectedAnswerOption)
                  .WithMany()
                  .HasForeignKey(sr => sr.SelectedAnswerOptionId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });

        // Global UTC DateTime value converters for PostgreSQL timestamptz compatibility
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()),
            v => !v.HasValue ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
