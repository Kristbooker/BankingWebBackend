using Microsoft.AspNetCore.Mvc; // httpGet,httpPost
using Microsoft.EntityFrameworkCore; // context
using Microsoft.IdentityModel.Tokens; // JwtSecurityToken
using System.IdentityModel.Tokens.Jwt; // JwtSecurityTokenHandler
using System.Security.Claims; // ClaimsIdentity
using System.Text; // Encoding
using System.Security.Cryptography; // hash
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BankingContext _context;
    private readonly IConfiguration _configuration; // jwt:Key

    public AuthController(BankingContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    //register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {   
        //check email
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already in use.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password) ?? throw new ArgumentNullException(nameof(dto.Password))
        };
        //add new user
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email); //search user
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = GenerateJwtToken(user);

        //return token
        return Ok(new AuthResponseDto { Token = token });
    }

    //hash password
    private string HashPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }


    //verify password
    private bool VerifyPassword(string password, string hash)
    {
        //check if password match the hash
        if (password == null) throw new ArgumentNullException(nameof(password));
        return HashPassword(password) == hash;
    }

    //gen jwt token
    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("id", user.Id.ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer"),
            audience: _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience"),
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
