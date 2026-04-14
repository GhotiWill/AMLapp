using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AMLapp.Models;

public partial class _43pBardakovExamPrepContext : DbContext
{
    public _43pBardakovExamPrepContext()
    {
    }

    public _43pBardakovExamPrepContext(DbContextOptions<_43pBardakovExamPrepContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserTest> UserTests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=edu.pg.ngknn.ru;Port=5442;Database=43P_Bardakov_ExamPrep;Username=43P;Password=444444");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("answers_pk");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Answers_id_seq1\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Question).HasColumnName("question");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Text)
                .HasColumnType("character varying")
                .HasColumnName("text");

            entity.HasOne(d => d.QuestionNavigation).WithMany(p => p.Answers)
                .HasForeignKey(d => d.Question)
                .HasConstraintName("answers_questions_fk");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questions_pk");

            entity.HasIndex(e => new { e.Test, e.NumberInTest }, "questions_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Questions_id_seq1\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.NumberInTest).HasColumnName("number_in_test");
            entity.Property(e => e.Test).HasColumnName("test");
            entity.Property(e => e.Text)
                .HasColumnType("character varying")
                .HasColumnName("text");

            entity.HasOne(d => d.TestNavigation).WithMany(p => p.Questions)
                .HasForeignKey(d => d.Test)
                .HasConstraintName("questions_tests_fk");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tests_pk");

            entity.HasIndex(e => e.Name, "tests_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Tests_id_seq1\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("newtable_pk");

            entity.HasIndex(e => e.Login, "users_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Users_id_seq1\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.Login)
                .HasColumnType("character varying")
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_answer_pk");

            entity.ToTable("User_answer");

            entity.HasIndex(e => new { e.User, e.Answer }, "user_answer_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('user_answer_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.AnswerNavigation).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.Answer)
                .HasConstraintName("user_answer_answers_fk");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.User)
                .HasConstraintName("user_answer_users_fk");
        });

        modelBuilder.Entity<UserTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_test_pk");

            entity.ToTable("user_test");

            entity.HasIndex(e => new { e.User, e.Test }, "user_test_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsComplete).HasColumnName("is_complete");
            entity.Property(e => e.Test).HasColumnName("test");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.TestNavigation).WithMany(p => p.UserTests)
                .HasForeignKey(d => d.Test)
                .HasConstraintName("user_test_tests_fk");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.UserTests)
                .HasForeignKey(d => d.User)
                .HasConstraintName("user_test_users_fk");
        });
        modelBuilder.HasSequence("Answers_id_seq").HasMax(2147483647L);
        modelBuilder.HasSequence("Questions_id_seq").HasMax(2147483647L);
        modelBuilder.HasSequence("Tests_id_seq").HasMax(2147483647L);
        modelBuilder.HasSequence("user_answer_id_seq").HasMax(2147483647L);
        modelBuilder.HasSequence("Users_id_seq").HasMax(2147483647L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
