using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Controllers
{
    [Produces("application/json")]
    [Route("api/auth/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthContext _context;

        public AuthController(AuthContext context)
        {
            _context = context;
        }

        // GET: api/Auth
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Auth/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(long id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Auth/5
        //Сделать проверку на роли
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(long id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Auth
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsers", new { id = users.Id }, users);
        }
        
        // DELETE: api/Auth/5
        //Добавить проверку на роли
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Users>> DeleteUsers(long id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return users;
        }

        [HttpPost("token")]
        public ActionResult<String> Token(LoginAndPassword info)
        {
            var identity = GetIdentity(info.Login, info.Password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }
 
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };
 
            return Ok(response);
        }
 
        private ClaimsIdentity GetIdentity(string username, string password)
        {
            Users person = _context.Users.FirstOrDefault(x => x.Login == username && x.Password == password);
            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, person.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }

        [Authorize]
        [HttpPost("getlogin")]
        public IActionResult GetLogin()
        {
            Users person = _context.Users.FirstOrDefault(x => x.Login == User.Identity.Name);
            var response = new
            {
                login = User.Identity.Name,
                authorized = User.Identity.IsAuthenticated,
                id = person.Id,
                role = person.Role
            };
            return Ok(response);
        }
    }
}
