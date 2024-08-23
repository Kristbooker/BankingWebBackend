public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } 
    public decimal Balance { get; set; }
    public ICollection<Transaction> SentTransactions { get; set; } // for transaction sent history
    public ICollection<Transaction> ReceivedTransactions { get; set; } // for transaction received history
}
