using FitnessClub.Models;
using FitnessClub.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<MemberSubscription> MemberSubscriptions { get; set; }
        public DbSet<DayPass> DayPasses { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<TrainerUnavailability> TrainerUnavailabilities { get; set; }
        public DbSet<BookedSession> BookedSessions { get; set; }
        public DbSet<GroupClass> GroupClasses { get; set; }
        public DbSet<GroupClassSchedule> GroupClassSchedules { get; set; }
        public DbSet<GroupClassEnrollment> GroupClassEnrollments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventEnrollment> EventEnrollments { get; set; }
        public DbSet<BodyMeasurement> BodyMeasurements { get; set; }
        public DbSet<TrainerNote> TrainerNotes { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            builder.Entity<Member>()
                .HasOne(m => m.User)
                .WithOne(u => u.Member)
                .HasForeignKey<Member>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Trainer>()
                .HasOne(t => t.User)
                .WithOne(u => u.Trainer)
                .HasForeignKey<Trainer>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TrainerAvailability>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Availability)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TrainerUnavailability>()
                .HasOne(u => u.Trainer)
                .WithMany(t => t.Unavailabilities)
                .HasForeignKey(u => u.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MemberSubscription>()
                .HasOne(s => s.Member)
                .WithMany(m => m.Subscriptions)
                .HasForeignKey(s => s.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MemberSubscription>()
                .HasOne(s => s.Plan)
                .WithMany(p => p.MemberSubscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DayPass>()
                .HasOne(d => d.Member)
                .WithMany(m => m.DayPasses)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DayPass>()
                .HasOne(d => d.Plan)
                .WithMany()
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BookedSession>()
                .HasOne(s => s.Member)
                .WithMany(m => m.BookedSessions)
                .HasForeignKey(s => s.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BookedSession>()
                .HasOne(s => s.Trainer)
                .WithMany(t => t.BookedSessions)
                .HasForeignKey(s => s.TrainerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<GroupClass>()
                .HasOne(g => g.Trainer)
                .WithMany(t => t.GroupClasses)
                .HasForeignKey(g => g.TrainerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<GroupClassSchedule>()
                .HasOne(s => s.GroupClass)
                .WithMany(g => g.Schedules)
                .HasForeignKey(s => s.GroupClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupClassEnrollment>()
                .HasOne(e => e.Schedule)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupClassEnrollment>()
                .HasOne(e => e.Member)
                .WithMany(m => m.ClassEnrollments)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<EventEnrollment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrollments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<EventEnrollment>()
                .HasOne(e => e.Member)
                .WithMany(m => m.EventEnrollments)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BodyMeasurement>()
                .HasOne(b => b.Member)
                .WithMany(m => m.BodyMeasurements)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TrainerNote>()
                .HasOne(n => n.Trainer)
                .WithMany(t => t.Notes)
                .HasForeignKey(n => n.TrainerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TrainerNote>()
                .HasOne(n => n.Member)
                .WithMany(m => m.TrainerNotes)
                .HasForeignKey(n => n.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Order>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "role-member-001", Name = "member", NormalizedName = "MEMBER" },
                new IdentityRole { Id = "role-trainer-002", Name = "trainer", NormalizedName = "TRAINER" },
                new IdentityRole { Id = "role-admin-003", Name = "admin", NormalizedName = "ADMIN" }
            );

            builder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = 1, Name = "Day Pass", Price = 25, DurationDays = 1, Description = "Acces pentru o zi.", IncludesGroupClasses = false, IsActive = true, Type = SubscriptionPlanType.DayPass },
                new SubscriptionPlan { Id = 2, Name = "Lunar", Price = 150, DurationDays = 30, Description = "Abonament lunar cu acces nelimitat la sala si clase de grup.", IncludesGroupClasses = true, IsActive = true, Type = SubscriptionPlanType.Subscription },
                new SubscriptionPlan { Id = 3, Name = "Trimestrial", Price = 400, DurationDays = 90, Description = "Abonament trimestrial cu acces nelimitat la sala si clase de grup.", IncludesGroupClasses = true, IsActive = true, Type = SubscriptionPlanType.Subscription },
                new SubscriptionPlan { Id = 4, Name = "Anual", Price = 1200, DurationDays = 365, Description = "Abonament anual cu acces nelimitat la toate facilitatile.", IncludesGroupClasses = true, IsActive = true, Type = SubscriptionPlanType.Subscription }
            );

            builder.Entity<ProductCategory>().HasData(
                new ProductCategory { Id = 1, Name = "Suplimente" },
                new ProductCategory { Id = 2, Name = "Snacks" },
                new ProductCategory { Id = 3, Name = "Echipament" }
            );

            builder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 2, Name = "Protein Bar", Description = "Baton proteic cu ciocolata, 60g.", Price = 12, Stock = 50, IsActive = true },
                new Product { Id = 2, CategoryId = 1, Name = "Whey Protein", Description = "Pudra proteica din zer, 1kg, aroma vanilie.", Price = 250, Stock = 20, IsActive = true },
                new Product { Id = 3, CategoryId = 3, Name = "Resistance Band", Description = "Banda elastica de rezistenta, set 3 niveluri.", Price = 45, Stock = 30, IsActive = true },
                new Product { Id = 4, CategoryId = 1, Name = "Creatina Monohidrat", Description = "Creatina pura, 300g.", Price = 80, Stock = 25, IsActive = true }
            );

            builder.Entity<Quote>().HasData(
                new Quote { Id = 1, Text = "The only bad workout is the one that didn't happen.", Author = "Unknown", IsActive = true },
                new Quote { Id = 2, Text = "Progress, not perfection.", Author = "Unknown", IsActive = true },
                new Quote { Id = 3, Text = "Push yourself because no one else is going to do it for you.", Author = "Unknown", IsActive = true },
                new Quote { Id = 4, Text = "Success starts with self-discipline.", Author = "Unknown", IsActive = true },
                new Quote { Id = 5, Text = "Your body can stand almost anything. It's your mind you have to convince.", Author = "Unknown", IsActive = true }
            );
        }
    }
}
