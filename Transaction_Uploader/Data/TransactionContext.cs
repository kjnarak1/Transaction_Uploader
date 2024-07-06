using Microsoft.EntityFrameworkCore;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Data
{
    public class TransactionContext : DbContext
    {
        public DbSet<Transaction> Transaction { get; set; }

        public TransactionContext(DbContextOptions<TransactionContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.TransactionId)
                      .IsRequired()
                      .HasColumnName("Transaction_Id")
                      .HasMaxLength(50);

                entity.Property(e => e.Amount)
                      .IsRequired()
                      .HasColumnName("Amount")
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CurrencyCode)
                      .IsRequired()
                      .HasColumnName("Currency_Code")
                      .HasMaxLength(10);

                entity.Property(e => e.TransactionDate)
                      .IsRequired()
                      .HasColumnName("Transaction_Date");

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasColumnName("Status")
                      .HasMaxLength(20);
            });
        }
    }
}
