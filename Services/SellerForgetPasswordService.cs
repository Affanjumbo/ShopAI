using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.CustomerForgetPasswordDTOs; // Reusing customer DTOs
using ShopAI.Models;
using BCrypt.Net;

namespace ShopAI.Services
{
    public class SellerForgetPasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public SellerForgetPasswordService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Reusing customer's ForgetPasswordRequestDto
        public async Task<string> SendOtpAsync(ForgetPasswordRequestDto request)
        {
            // Check against Seller table instead of Customer
            var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.Email == request.Email);
            if (seller == null)
                return "Seller not found";

            // Same OTP generation logic
            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(3);

            // Using same PasswordResetOtp table but for sellers
            var existingOtp = await _context.PasswordResetOtps
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingOtp != null)
            {
                existingOtp.OtpCode = otp;
                existingOtp.ExpiryTime = expiryTime;
                existingOtp.IsUsed = false;
                _context.PasswordResetOtps.Update(existingOtp);
            }
            else
            {
                var otpEntry = new PasswordResetOtp
                {
                    Email = request.Email,
                    OtpCode = otp,
                    ExpiryTime = expiryTime,
                    IsUsed = false
                };
                await _context.PasswordResetOtps.AddAsync(otpEntry);
            }

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                request.Email,
                "Your ShopAI Seller OTP Code",
                $"Your OTP is {otp}. It will expire in 3 minutes."
            );

            return "OTP sent successfully";
        }

        // Reusing customer's OtpVerificationDto
        public async Task<bool> VerifyOtpAsync(OtpVerificationDto request)
        {
            // Same verification logic but we'll validate seller exists
            var sellerExists = await _context.Sellers.AnyAsync(s => s.Email == request.Email);
            if (!sellerExists) return false;

            var otpEntry = await _context.PasswordResetOtps
                .FirstOrDefaultAsync(o => o.Email == request.Email &&
                                       o.OtpCode == request.OtpCode &&
                                       !o.IsUsed);

            return otpEntry != null && otpEntry.ExpiryTime >= DateTime.UtcNow;
        }

        // Reusing customer's ResetPasswordDto
        public async Task<string> ResetPasswordAsync(ResetPasswordDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return "Passwords do not match";

            // Verify OTP first
            var otpEntry = await _context.PasswordResetOtps
                .FirstOrDefaultAsync(o => o.Email == request.Email && !o.IsUsed);

            if (otpEntry == null || otpEntry.ExpiryTime < DateTime.UtcNow)
                return "Invalid or expired OTP";

            // Find the seller instead of customer
            var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.Email == request.Email);
            if (seller == null)
                return "Seller not found";

            // Same password hashing
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Update seller's password
            seller.Password = hashedPassword;
            _context.Sellers.Update(seller);

            // Mark OTP as used
            otpEntry.IsUsed = true;
            _context.PasswordResetOtps.Update(otpEntry);

            await _context.SaveChangesAsync();

            return "Password reset successfully";
        }
    }
}