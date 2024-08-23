using Microsoft.EntityFrameworkCore; // context

public class BankingContext : DbContext
{
    public DbSet<User> Users { get; set; } // user table
    public DbSet<Transaction> Transactions { get; set; } // transaction table

    public BankingContext(DbContextOptions<BankingContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromUser) //  one transaction has one user as sender 
            .WithMany(u => u.SentTransactions) //  one user can have many transactions sent
            .HasForeignKey(t => t.FromUserId) // FromUserId is a foreign key in Transaction table that references User.Id
            .OnDelete(DeleteBehavior.Restrict); // 

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToUser) // one transaction has one user as receiver
            .WithMany(u => u.ReceivedTransactions) // one user can have many transactions received
            .HasForeignKey(t => t.ToUserId) // ToUserId is a foreign key in Transaction table that references User.Id
            .OnDelete(DeleteBehavior.Restrict);
    }
    //user one to many transaction
}
