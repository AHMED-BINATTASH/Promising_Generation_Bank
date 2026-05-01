using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<SavingsTransaction> SavingsTransactions{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuration for Parent Entity
            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);

                //// إعدادات الحقول المالية
                //entity.Property(p => p.TotalFamilyBalance).HasPrecision(18, 2);
                //entity.Property(p => p.EarnedThisWeek).HasPrecision(18, 2);

                // Relation: Parent (1) -> (M) Children
                entity.HasMany(p => p.Children)
                      .WithOne(c => c.Parent)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Quests)
                 .WithOne(q => q.Parent)
                 .HasForeignKey(q => q.ParentId)
                 .OnDelete(DeleteBehavior.NoAction);


            });

            // 2. Configuration for Child Entity
            modelBuilder.Entity<Child>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.AvatarUrl).HasMaxLength(500);

                // إعداد الحقل المالي
                entity.Property(c => c.SavingsBalance).HasPrecision(18, 2);

                // Relations



                entity.HasMany(c => c.Quests)
                      .WithOne(q => q.Child)
                      .HasForeignKey(q => q.ChildId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.SavingsGoals)
                      .WithOne(sg => sg.Child)
                      .HasForeignKey(sg => sg.ChildId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Transactions)
                      .WithOne(t => t.Child)
                      .HasForeignKey(t => t.ChildId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // 3. Configuration for Quest Entity
            modelBuilder.Entity<Quest>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Title).IsRequired().HasMaxLength(200);
                entity.Property(q => q.Description).HasMaxLength(1000);

                // إعداد الحقل المالي الجديد
                entity.Property(q => q.Amount).HasPrecision(18, 2);

                entity.Property(q => q.Status).HasConversion<string>(); // حفظ حالة المهمة كنص
                entity.Property(q => q.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // 4. Configuration for SavingsGoal Entity
            modelBuilder.Entity<SavingsGoal>(entity =>
            {
                entity.HasKey(sg => sg.Id);
                entity.Property(sg => sg.Name).IsRequired().HasMaxLength(150);

                // إعدادات الحقول المالية
                entity.Property(sg => sg.TargetAmount).HasPrecision(18, 2);
                entity.Property(sg => sg.CurrentAmount).HasPrecision(18, 2);

                // ⚠️ تجاهل الخاصية المحسوبة حتى لا يحاول EF Core إنشاء عمود لها في قاعدة البيانات
                entity.Ignore(sg => sg.ProgressPercentage);
            });

            // 5. Configuration for Transaction Entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ActivityName).IsRequired().HasMaxLength(150);

                // إعداد الحقل المالي
                entity.Property(t => t.CashAmount).HasPrecision(18, 2);
                entity.Property(t => t.Date).HasDefaultValueSql("GETUTCDATE()");

                modelBuilder.Entity<Transaction>()
                   .HasOne(t => t.Quest)
                   .WithMany(q => q.Transactions)
                   .HasForeignKey(t => t.QuestId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.NoAction);
            });



            modelBuilder.Entity<SavingsTransaction>(entity =>
            {
                entity.ToTable("SavingsTransactions");


                entity.HasKey(t => t.Id);


                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(t => t.TransactionDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(t => t.ChildId)
                    .IsRequired(false).HasDefaultValue(null);

                entity.Property(t => t.Notes)
                    .HasMaxLength(500);

                entity.HasOne(t => t.SavingsGoal)
                  .WithMany(g => g.Transactions) // هنا نحدد اسم القائمة الموجودة في الهدف
                  .HasForeignKey(t => t.SavingsGoalId);

              

            });
        }
    }
}

