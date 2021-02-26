using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace TestingAndCertificationSystem
{
    public partial class TestingSystemDBContext : DbContext
    {
        public TestingSystemDBContext()
        {
        }

        public TestingSystemDBContext(DbContextOptions<TestingSystemDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdditionalTask> AdditionalTask { get; set; }
        public virtual DbSet<Choice> Choice { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Question> Question { get; set; }
        public virtual DbSet<QuestionAnswer> QuestionAnswer { get; set; }
        public virtual DbSet<Registration> Registration { get; set; }
        public virtual DbSet<Test> Test { get; set; }
        public virtual DbSet<TestResults> TestResults { get; set; }
        public virtual DbSet<VerifiedUsers> VerifiedUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestingSystemDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdditionalTask>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.RecipientEmail)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Choice>(entity =>
            {
                entity.Property(e => e.Text)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Choice)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("FK__Choice__Question__0D0FEE32");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.EstablishmentDate).HasColumnType("date");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ShortName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WebsiteUrl)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.QuestionType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.Question)
                    .HasForeignKey(d => d.TestId)
                    .HasConstraintName("FK__Question__TestId__0C1BC9F9");
            });

            modelBuilder.Entity<QuestionAnswer>(entity =>
            {
                entity.HasOne(d => d.Choice)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.ChoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Choic__23F3538A");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Quest__25DB9BFC");

                entity.HasOne(d => d.Registration)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.RegistrationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Regis__24E777C3");

                entity.HasOne(d => d.TestResult)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.TestResultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__TestR__26CFC035");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Instruction)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.IsPrivate)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Link).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TestAuthorId)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.AdditionalTask)
                    .WithMany(p => p.Test)
                    .HasForeignKey(d => d.AdditionalTaskId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Test__Additional__53D770D6");
            });

            modelBuilder.Entity<TestResults>(entity =>
            {
                entity.HasOne(d => d.Registration)
                    .WithMany(p => p.TestResults)
                    .HasForeignKey(d => d.RegistrationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TestResul__Regis__42E1EEFE");
            });

            modelBuilder.Entity<VerifiedUsers>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.VerifiedUsers)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__VerifiedU__TestI__1F63A897");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
