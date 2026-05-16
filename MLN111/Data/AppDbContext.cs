using Microsoft.EntityFrameworkCore;
using MLN111.Entities;

namespace MLN111.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<QuizRoom> QuizRooms => Set<QuizRoom>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizChoice> QuizChoices => Set<QuizChoice>();
    public DbSet<QuizParticipant> QuizParticipants => Set<QuizParticipant>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<QuizRoom>(e =>
        {
            e.ToTable("quiz_rooms");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.JoinCode).HasMaxLength(12).IsRequired();
            e.HasIndex(x => x.JoinCode).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SecondsPerQuestion).HasDefaultValue(QuizSessionDefaults.SecondsPerQuestion);
            e.HasOne(x => x.CreatedBy)
                .WithMany(u => u.CreatedQuizRooms)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizQuestion>(e =>
        {
            e.ToTable("quiz_questions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            e.HasIndex(x => new { x.QuizRoomId, x.OrderIndex }).IsUnique();
            e.HasOne(x => x.QuizRoom)
                .WithMany(r => r.Questions)
                .HasForeignKey(x => x.QuizRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizChoice>(e =>
        {
            e.ToTable("quiz_choices");
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Question)
                .WithMany(q => q.Choices)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizParticipant>(e =>
        {
            e.ToTable("quiz_participants");
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayNameSnapshot).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.QuizRoomId, x.UserId }).IsUnique();
            e.HasIndex(x => new { x.QuizRoomId, x.TotalScore });
            e.HasOne(x => x.QuizRoom)
                .WithMany(r => r.Participants)
                .HasForeignKey(x => x.QuizRoomId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.QuizParticipations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAnswer>(e =>
        {
            e.ToTable("quiz_answers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ParticipantId, x.QuestionId }).IsUnique();
            e.HasOne(x => x.Participant)
                .WithMany(p => p.Answers)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Choice)
                .WithMany()
                .HasForeignKey(x => x.ChoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
