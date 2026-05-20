using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Transport_Management_System.Data;
using Transport_Management_System.Models;
using Transport_Management_System.Models.ViewModels;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IBaseRepository<Role> _roleRepo;
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(IUserRepo userRepo, IBaseRepository<Role> roleRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _userRepo.GetByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var token = Guid.NewGuid().ToString();
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Address = model.Address,
                    RoleId = 2, // Hardcoded "User" role ID from AppDbContext seed data
                    IsEmailVerified = false,
                    EmailVerificationToken = token,
                    EmailVerificationTokenExpiry = DateTime.Now.AddHours(24),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Hash password
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

                await _userRepo.AddAsync(user);
                await _userRepo.SaveAsync();

                // Send Verification Email
                var verifyLink = Url.Action("VerifyEmail", "Account", new { userId = user.UserId, token = token }, Request.Scheme);
                
                var emailBody = $@"
                    <div style='font-family: ""Inter"", sans-serif; background-color: #0f172a; color: #f8fafc; padding: 40px; border-radius: 16px; max-width: 600px; margin: 0 auto; box-shadow: 0 10px 25px rgba(0,0,0,0.5); border: 1px solid rgba(255,255,255,0.05);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #0ea5e9; font-weight: 800; font-size: 28px; margin: 0;'>TMS PRO</h1>
                            <p style='color: #64748b; margin: 5px 0 0 0; font-size: 14px;'>Smart Transport Management System</p>
                        </div>
                        <h2 style='font-size: 20px; font-weight: 700; text-align: center; color: #ffffff;'>Verify Your Email Address</h2>
                        <p style='color: #94a3b8; font-size: 15px; line-height: 1.6; text-align: center;'>Thank you for signing up for TMS PRO! To activate your account and access your dashboard, please verify your email address by clicking the button below.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verifyLink}' style='background: linear-gradient(135deg, #4f46e5, #4338ca); color: #ffffff !important; text-decoration: none; padding: 12px 30px; font-size: 15px; font-weight: 600; border-radius: 8px; box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3); display: inline-block;'>Verify Email Address</a>
                        </div>
                        <p style='color: #ef4444; font-size: 13px; text-align: center; margin-top: 20px;'>Note: This verification link will expire in 24 hours.</p>
                        <hr style='border: 0; border-top: 1px solid rgba(255,255,255,0.1); margin: 30px 0;'>
                        <p style='color: #64748b; font-size: 12px; text-align: center; margin: 0;'>If you did not request this account, you can safely ignore this email.</p>
                    </div>";

                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Verify Your Email - TMS PRO", emailBody);
                    TempData["UnverifiedEmail"] = user.Email;
                    return RedirectToAction("EmailVerificationPending");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "User created but failed to send verification email. Please try logging in to resend verification link.");
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userRepo.GetByEmailAsync(model.Email);

                if (user != null)
                {
                    // Verify password
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        // Enforce Email Verification
                        if (!user.IsEmailVerified)
                        {
                            ModelState.AddModelError(string.Empty, "Please verify your email before logging in.");
                            ViewBag.ShowResendVerification = true;
                            ViewBag.UnverifiedEmail = user.Email;
                            return View(model);
                        }

                        // Update Last Login
                        user.LastLogIN = DateTime.Now;
                        _userRepo.Update(user);
                        await _userRepo.SaveAsync();

                        // Create Claims for Cookie Auth
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // Redirect based on role
                        if (user.Role?.RoleName == "Admin")
                        {
                            return RedirectToAction("Index", "Users");
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: Account/EmailVerificationPending
        public IActionResult EmailVerificationPending()
        {
            var email = TempData["UnverifiedEmail"] as string ?? "";
            ViewBag.Email = email;
            return View();
        }

        // GET: Account/VerifyEmail
        public async Task<IActionResult> VerifyEmail(int userId, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Status = "InvalidToken";
                return View("VerifyEmailSuccess");
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Status = "UserNotFound";
                return View("VerifyEmailSuccess");
            }

            if (user.IsEmailVerified)
            {
                ViewBag.Status = "AlreadyVerified";
                return View("VerifyEmailSuccess");
            }

            if (user.EmailVerificationToken != token || user.EmailVerificationTokenExpiry < DateTime.Now)
            {
                ViewBag.Status = "ExpiredOrInvalid";
                return View("VerifyEmailSuccess");
            }

            // Successfully Verified
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            ViewBag.Status = "Success";
            return View("VerifyEmailSuccess");
        }

        // POST: Account/ResendVerificationEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email address is required.";
                return RedirectToAction("Login");
            }

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || user.IsEmailVerified)
            {
                TempData["ErrorMessage"] = "Unable to send verification link.";
                return RedirectToAction("Login");
            }

            var token = Guid.NewGuid().ToString();
            user.EmailVerificationToken = token;
            user.EmailVerificationTokenExpiry = DateTime.Now.AddHours(24);
            _userRepo.Update(user);
            await _userRepo.SaveAsync();

            var verifyLink = Url.Action("VerifyEmail", "Account", new { userId = user.UserId, token = token }, Request.Scheme);
            
            var emailBody = $@"
                <div style='font-family: ""Inter"", sans-serif; background-color: #0f172a; color: #f8fafc; padding: 40px; border-radius: 16px; max-width: 600px; margin: 0 auto; box-shadow: 0 10px 25px rgba(0,0,0,0.5); border: 1px solid rgba(255,255,255,0.05);'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #0ea5e9; font-weight: 800; font-size: 28px; margin: 0;'>TMS PRO</h1>
                        <p style='color: #64748b; margin: 5px 0 0 0; font-size: 14px;'>Smart Transport Management System</p>
                    </div>
                    <h2 style='font-size: 20px; font-weight: 700; text-align: center; color: #ffffff;'>Verify Your Email Address</h2>
                    <p style='color: #94a3b8; font-size: 15px; line-height: 1.6; text-align: center;'>Here is your requested verification link. Please active your account by clicking the button below.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verifyLink}' style='background: linear-gradient(135deg, #4f46e5, #4338ca); color: #ffffff !important; text-decoration: none; padding: 12px 30px; font-size: 15px; font-weight: 600; border-radius: 8px; box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3); display: inline-block;'>Verify Email Address</a>
                    </div>
                    <p style='color: #ef4444; font-size: 13px; text-align: center; margin-top: 20px;'>Note: This verification link will expire in 24 hours.</p>
                    <hr style='border: 0; border-top: 1px solid rgba(255,255,255,0.1); margin: 30px 0;'>
                    <p style='color: #64748b; font-size: 12px; text-align: center; margin: 0;'>If you did not request this account, you can safely ignore this email.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Verify Your Email - TMS PRO", emailBody);
                TempData["SuccessMessage"] = "A new verification link has been sent to your email inbox.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed to send verification email. Please try again later.";
            }

            return RedirectToAction("Login");
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepo.GetByEmailAsync(model.Email);
                if (user == null)
                {
                    // For security, don't reveal if user does not exist
                    TempData["SuccessMessage"] = "If an account exists, a 6-digit verification code has been sent to your email.";
                    return RedirectToAction("VerifyOtp", new { email = model.Email });
                }

                // Generate 6 digit numeric OTP
                var otp = new Random().Next(100000, 999999).ToString();
                
                user.PasswordResetOtp = otp;
                user.PasswordResetOtpExpiry = DateTime.Now.AddMinutes(15);
                user.PasswordResetOtpAttempts = 0;
                
                _userRepo.Update(user);
                await _userRepo.SaveAsync();

                var emailBody = $@"
                    <div style='font-family: ""Inter"", sans-serif; background-color: #0f172a; color: #f8fafc; padding: 40px; border-radius: 16px; max-width: 600px; margin: 0 auto; box-shadow: 0 10px 25px rgba(0,0,0,0.5); border: 1px solid rgba(255,255,255,0.05);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #0ea5e9; font-weight: 800; font-size: 28px; margin: 0;'>TMS PRO</h1>
                            <p style='color: #64748b; margin: 5px 0 0 0; font-size: 14px;'>Smart Transport Management System</p>
                        </div>
                        <h2 style='font-size: 20px; font-weight: 700; text-align: center; color: #ffffff;'>Password Reset Code</h2>
                        <p style='color: #94a3b8; font-size: 15px; line-height: 1.6; text-align: center;'>You have requested to reset your password. Use the verification code (OTP) below to proceed. Do not share this code with anyone.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); color: #0ea5e9; font-size: 32px; font-weight: 800; letter-spacing: 8px; padding: 15px 30px; border-radius: 12px; display: inline-block;'>{otp}</div>
                        </div>
                        <p style='color: #ef4444; font-size: 13px; text-align: center; margin-top: 20px;'>Note: This OTP code is single-use and will expire in 15 minutes.</p>
                        <hr style='border: 0; border-top: 1px solid rgba(255,255,255,0.1); margin: 30px 0;'>
                        <p style='color: #64748b; font-size: 12px; text-align: center; margin: 0;'>If you did not request a password reset, please secure your account immediately.</p>
                    </div>";

                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Reset Password OTP - TMS PRO", emailBody);
                    TempData["SuccessMessage"] = "A 6-digit verification code has been sent to your email.";
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send reset email. Please try again later.");
                    return View(model);
                }

                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }

            return View(model);
        }

        // GET: Account/VerifyOtp
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View(new VerifyOtpViewModel { Email = email });
        }

        // POST: Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepo.GetByEmailAsync(model.Email);
                if (user == null || string.IsNullOrEmpty(user.PasswordResetOtp))
                {
                    ModelState.AddModelError(string.Empty, "Invalid request. Please request a new OTP code.");
                    return View(model);
                }

                // Brute force protection
                if (user.PasswordResetOtpAttempts >= 5 || user.PasswordResetOtpExpiry < DateTime.Now)
                {
                    user.PasswordResetOtp = null;
                    user.PasswordResetOtpExpiry = null;
                    user.PasswordResetOtpAttempts = 0;
                    _userRepo.Update(user);
                    await _userRepo.SaveAsync();

                    ModelState.AddModelError(string.Empty, "This verification code has expired or has been locked due to too many failed attempts.");
                    return View(model);
                }

                if (user.PasswordResetOtp == model.Otp)
                {
                    // Valid OTP! Generate a temporary signature token to allow Password Reset view
                    var resetToken = Guid.NewGuid().ToString();
                    user.PasswordResetOtp = resetToken; // Reuse column to store secure redirect token
                    _userRepo.Update(user);
                    await _userRepo.SaveAsync();

                    return RedirectToAction("ResetPassword", new { email = user.Email, token = resetToken });
                }

                // Failed attempt
                user.PasswordResetOtpAttempts++;
                _userRepo.Update(user);
                await _userRepo.SaveAsync();

                ModelState.AddModelError(string.Empty, $"Incorrect verification code. Attempts remaining: {5 - user.PasswordResetOtpAttempts}");
            }

            return View(model);
        }

        // GET: Account/ResetPassword
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword");
            }

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || user.PasswordResetOtp != token || user.PasswordResetOtpExpiry < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Password reset link is invalid or expired. Please start over.";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepo.GetByEmailAsync(model.Email);
                if (user == null || user.PasswordResetOtp != model.Token || user.PasswordResetOtpExpiry < DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Your session has expired or is invalid. Please request a new code.");
                    return View(model);
                }

                // Update Password Hash
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
                
                // Clear OTP Fields
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                user.PasswordResetOtpAttempts = 0;
                
                _userRepo.Update(user);
                await _userRepo.SaveAsync();

                TempData["SuccessMessage"] = "Your password has been successfully reset. Please log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
