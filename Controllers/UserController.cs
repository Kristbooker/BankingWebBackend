using Microsoft.AspNetCore.Mvc; // httpGet,httpPost
using Microsoft.EntityFrameworkCore; // context
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; //authorize
using System.Security.Claims; //manage data current user

[ApiController]
[Route("api/[controller]")]
[Authorize] // authorize first b4 use these services
public class UserController : ControllerBase
{
    private readonly BankingContext _context;

    public UserController(BankingContext context)
    {
        _context = context;
    }

    // [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        //get id from current user's token
        var currentUserId = int.Parse(User.Claims.First(c => c.Type == "id").Value);

        // check current user access own data
        if (currentUserId != id)
        {
            //return Unauthorized();
            return Forbid(); 
        }
        // select user
        var user = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Balance = u.Balance
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // get user by email
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        //get email from token
        var currentUserEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        // check current user access own data
        if (currentUserEmail != email)
        {
            //return Unauthorized();
            return Forbid(); 
        }

        var user = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Balance = u.Balance
            })
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    //deposit
    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(int id, [FromBody] DepositWithdrawDto dto)
    {
        //check deposit amount
        if (dto.Amount <= 0)
            return BadRequest("Deposit amount must be positive.");

        //find user by id
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Balance += dto.Amount; //add deposit
        _context.Users.Update(user); //update

        //add transaction for transaction history
        var transaction = new Transaction
        {
            Date = DateTime.Now,
            Action = "Deposit",
            Amount = dto.Amount,
            RemainingBalance = user.Balance,
            FromUserId = null, // No sender
            ToUserId = user.Id
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return Ok();
    }

    //withdraw
    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, [FromBody] DepositWithdrawDto dto)
    {
        //check withdraw amount
        if (dto.Amount <= 0)
            return BadRequest("Withdrawal amount must be positive.");

        //find user by id
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        //check user's balance
        if (user.Balance < dto.Amount)
            return BadRequest("Insufficient funds.");

        //withdraw user's balance
        user.Balance -= dto.Amount;
        _context.Users.Update(user);

        //add transaction for transaction history
        var transaction = new Transaction
        {
            Date = DateTime.Now,
            Action = "Withdraw",
            Amount = dto.Amount,
            RemainingBalance = user.Balance,
            FromUserId = user.Id,
            ToUserId = user.Id // No receiver
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return Ok();
    }
}
