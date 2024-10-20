using Member_Registration.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Member_Registration.Controllers
{
    public class AccountController : Controller
    {
        private readonly iBlueAnts_MembersContext _context;

        // AES Encryption Key (padded to 32 characters for AES-256)
        private string encryptionKey = "G0Z3oLNL9O5Kbsr7this";

        // Derive a valid 32-byte key using PBKDF2
        private byte[] GetEncryptionKey()
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(encryptionKey, new byte[16], 10000))
            {
                return rfc2898.GetBytes(32); // Generate a 256-bit key (32 bytes)
            }
        }


        public AccountController(iBlueAnts_MembersContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                // Hash the password
                var hashedPassword = HashPassword(model.Password);

                // Encrypt the username
                var encryptedUserName = Encrypt(model.UserName);

                // Create the user object with encrypted username and hashed password
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = encryptedUserName,
                    Password = hashedPassword
                };

                // Save user to the database
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                // Check user credentials (you already have this logic)
                var user = _context.Users.FirstOrDefault(u => u.UserName == Encrypt(model.UserName));
                if (user != null && VerifyPasswordHash(model.Password, user.Password))
                {
                    // Create the authentication cookie
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.UserName) // Store user information in claims
            };

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieScheme");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user
                    HttpContext.SignInAsync("MyCookieScheme", claimsPrincipal);

                    return RedirectToAction("ShowMembers", "Members");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieScheme");
            return RedirectToAction("Login", "Account"); // Redirect to home or login page
        }

        // Hash password using SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string Encrypt(string plainText)
        {
            byte[] iv = new byte[16]; // Initialization vector for AES (16 bytes)
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();  // Use derived key
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private bool VerifyPasswordHash(string enteredPassword, string storedPasswordHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var enteredHash = Convert.ToBase64String(hashBytes);

                // Compare hashed entered password with stored hashed password
                return enteredHash == storedPasswordHash;
            }
        }


    }
}
