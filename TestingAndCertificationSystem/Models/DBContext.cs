using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingAndCertificationSystem.Models
{
    public class DBContext : IdentityDbContext<UserIdentity>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public virtual DbSet<AdditionalTask> AdditionalTask { get; set; }
        public virtual DbSet<Choice> Choice { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Question> Question { get; set; }
        public virtual DbSet<QuestionAnswer> QuestionAnswer { get; set; }
        public virtual DbSet<Registration> Registration { get; set; }
        public virtual DbSet<Test> Test { get; set; }
        public virtual DbSet<TestResults> TestResults { get; set; }
        public virtual DbSet<VerifiedUser> VerifiedUsers { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }
        public virtual DbSet<VerifiedGroup> VerifiedGroups { get; set; }

    }
}
