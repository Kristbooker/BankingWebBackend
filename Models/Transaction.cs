public class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Action { get; set; } // Deposit, Withdraw, Transfer , Receive
    public decimal Amount { get; set; }
    public decimal RemainingBalance { get; set; }
    public int? FromUserId { get; set; } // can be null
    public User FromUser { get; set; }
    public int ToUserId { get; set; }
    public User ToUser { get; set; }
}
