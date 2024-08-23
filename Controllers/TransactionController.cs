using Microsoft.AspNetCore.Mvc; // httpGet,httpPost
using Microsoft.EntityFrameworkCore; // context


[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly BankingContext _context;

    public TransactionController(BankingContext context)
    {
        _context = context;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        //get sender and receiver 
        var fromUser = await _context.Users.FindAsync(request.FromUserId);
        var toUser = await _context.Users.FindAsync(request.ToUserId);

        if (fromUser == null || toUser == null)
            return NotFound("User(s) not found.");

        //check balance
        if (fromUser.Balance < request.Amount)
            return BadRequest("Insufficient funds.");

        //update balance sender and receiver
        fromUser.Balance -= request.Amount;
        toUser.Balance += request.Amount;

        _context.Users.Update(fromUser);
        _context.Users.Update(toUser);

        //add transaction for transaction history
        var transaction = new Transaction
        {
            Date = DateTime.Now,
            Action = "Transfer",
            Amount = request.Amount,
            RemainingBalance = fromUser.Balance,
            FromUserId = fromUser.Id,
            ToUserId = toUser.Id
        };
        var receiveTransaction = new Transaction
        {
            Date = DateTime.Now,
            Action = "Receive",
            Amount = request.Amount,
            RemainingBalance = toUser.Balance,
            FromUserId = fromUser.Id,
            ToUserId = toUser.Id
        };

        _context.Transactions.Add(transaction);
        _context.Transactions.Add(receiveTransaction);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // get user's transaction history by id
    [HttpGet("{userId}/history")]
    public async Task<IActionResult> GetTransactionHistory(int userId)
    {
        var transactions = await _context.Transactions
        // set search for transaction relate to user id
            .Where(t => (t.Action == "Deposit" && t.ToUserId == userId) || (t.Action == "Withdraw" && t.FromUserId == userId) || (t.Action == "Transfer" && t.FromUserId == userId) || (t.Action == "Receive" && t.ToUserId == userId))
            .Include(t => t.FromUser) //get FromUser data
            .Include(t => t.ToUser) //get ToUser data
            .OrderByDescending(t => t.Date) //order by newest date
            .Select(t => new TransactionDto
            {
                Date = t.Date,
                Action = t.Action,
                // User = t.FromUserId == userId ? t.FromUser.Username : t.ToUser.Username,
                RemainingBalance = t.RemainingBalance,
                Amount = t.Amount,
                From = t.FromUser == null ? "System" : t.FromUser.Username,
                To = t.ToUser.Username
            })
            .ToListAsync();

        return Ok(transactions);
    }


}