using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.CustomerForgetPasswordDTOs;
using ShopAI.Models;
using BCrypt.Net;

namespace ShopAI.Services
{
    public class CustomerForgetPasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public CustomerForgetPasswordService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Method to send OTP to customer's email
        public async Task<string> SendOtpAsync(ForgetPasswordRequestDto request)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (customer == null)
                return "Customer not found";

            // Generate OTP and set expiry time (3 minutes)
            var otp = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(3);

            // Check if an OTP already exists, otherwise create a new one
            var existingOtp = await _context.PasswordResetOtps.FirstOrDefaultAsync(x => x.Email == request.Email);
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

            // Use the injected EmailService to send the OTP email
            await _emailService.SendEmailAsync(
                request.Email,
                "Your ShopAI OTP Code",
                $"Your OTP is {otp}. It will expire in 3 minutes."
            );

            return "OTP sent successfully";
        }

        // Method to verify the OTP entered by the user
        public async Task<bool> VerifyOtpAsync(OtpVerificationDto request)
        {
            var otpEntry = await _context.PasswordResetOtps
                .FirstOrDefaultAsync(o => o.Email == request.Email && o.OtpCode == request.OtpCode && !o.IsUsed);

            if (otpEntry == null || otpEntry.ExpiryTime < DateTime.UtcNow)
                return false;

            return true;
        }

        // Method to reset the customer's password
        public async Task<string> ResetPasswordAsync(ResetPasswordDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return "Passwords do not match";

            // Verify OTP first
            var otpEntry = await _context.PasswordResetOtps
                .FirstOrDefaultAsync(o => o.Email == request.Email && !o.IsUsed);

            if (otpEntry == null || otpEntry.ExpiryTime < DateTime.UtcNow)
                return "Invalid or expired OTP";

            // Find the customer by email
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (customer == null)
                return "Customer not found";

            // Hash the new password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Update the customer's password with the hashed password
            customer.Password = hashedPassword;
            _context.Customers.Update(customer);

            // Mark OTP as used after successful reset
            otpEntry.IsUsed = true;
            _context.PasswordResetOtps.Update(otpEntry);

            await _context.SaveChangesAsync();

            return "Password reset successfully";
        }
    }
}
