using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onboarding.Models;
using System.Drawing;

namespace Onboarding.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        //public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<CourseTask> CourseTasks { get; set; }

		public DbSet<UserTask> UserTasks { get; set; }

		public DbSet<UserTestResult> UserTestResults { get; set; }

        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCourse>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCourses)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(uc => uc.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CourseTask>()
                .HasKey(ct => ct.Id);

            modelBuilder.Entity<CourseTask>()
                .HasOne(ct => ct.Task)
                .WithMany(t => t.CourseTasks)
                .HasForeignKey(ct => ct.TaskId);

            modelBuilder.Entity<CourseTask>()
                .HasOne(ct => ct.UserCourse)
                .WithMany(uc => uc.CourseTasks)
                .HasForeignKey(ct => ct.UserCourseId);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.GiverUser)
                .WithMany(u => u.GivenRewards)
                .HasForeignKey(r => r.Giver)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.ReceiverUser)
                .WithMany(u => u.ReceivedRewards)
                .HasForeignKey(r => r.Receiver)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Creator)
                .WithMany(u => u.Announcements)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.Mentor)
                .WithMany()
                .HasForeignKey(t => t.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            /*modelBuilder.Entity<Test>()
                .HasOne(te => te.Task)
                .WithMany(t => t.Tests)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Cascade);*/

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(te => te.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Task)
                .WithMany(t => t.Articles)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Link>()
                .HasOne(l => l.Task)
                .WithMany(t => t.Links)
                .HasForeignKey(l => l.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingParticipant>()
                .HasKey(mp => mp.Id);

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.Meeting)
                .WithMany(m => m.Participants)
                .HasForeignKey(mp => mp.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}
