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
        public virtual DbSet<QuestionInTest> QuestionInTest { get; set; }
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Choice__Question__0E6E26BF");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Description).IsUnicode(false);

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
            });

            modelBuilder.Entity<QuestionAnswer>(entity =>
            {
                entity.HasOne(d => d.Choice)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.ChoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Choic__160F4887");

                entity.HasOne(d => d.QuestionInTest)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.QuestionInTestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Quest__151B244E");

                entity.HasOne(d => d.Registration)
                    .WithMany(p => p.QuestionAnswer)
                    .HasForeignKey(d => d.RegistrationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionA__Regis__43D61337");
            });

            modelBuilder.Entity<QuestionInTest>(entity =>
            {
                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QuestionInTest)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionI__Quest__0A9D95DB");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.QuestionInTest)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__QuestionI__TestI__498EEC8D");
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

                entity.Property(e => e.DurationInMinutes)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Instruction)
                    .IsRequired()
                    .IsUnicode(false);

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
                    .HasConstraintName("FK__Test__Additional__4B7734FF");
            });

            modelBuilder.Entity<TestResults>(entity =>
            {
                entity.HasOne(d => d.QuestionAnswer)
                    .WithMany(p => p.TestResults)
                    .HasForeignKey(d => d.QuestionAnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TestResul__Quest__17F790F9");

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
                    .HasConstraintName("FK__VerifiedU__TestI__4A8310C6");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
