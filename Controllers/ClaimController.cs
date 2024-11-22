using Microsoft.AspNetCore.Authorization; // For role-based authorization.
using Microsoft.AspNetCore.Mvc; // For creating controllers and handling HTTP requests.
using Microsoft.EntityFrameworkCore; // For interacting with the database using Entity Framework Core.
using System.Security.Claims; // For accessing user identity claims.
using Sashiel_ST10028058_PROG6212_Part2.Data; // Importing the application's data context.
using AppClaim = Sashiel_ST10028058_PROG6212_Part2.Models.Claims; // Alias for the Claims model.
using OfficeOpenXml; // For working with Excel files.
using System.ComponentModel; 
using LicenseContext = OfficeOpenXml.LicenseContext; 

namespace Sashiel_ST10028058_PROG6212_Part2.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _dbContext; // Database context for accessing the application's data.
        private readonly long _maxFileSize = 5 * 1024 * 1024; // Maximum allowed file size (5 MB).
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" }; // Allowed file extensions.

        // Constructor to initialize the database context.
        public ClaimsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Render the claim submission form (restricted to Lecturers).
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View(); // Returns the view for submitting a claim.
        }

        // POST: Handle submission of a new claim with an optional supporting document.
        [HttpPost]
        public async Task<IActionResult> SubmitClaim(AppClaim claim, IFormFile document)
        {
            if (document != null && document.Length > 0)
            {
                var fileExtension = Path.GetExtension(document.FileName).ToLower();
                if (!_allowedExtensions.Contains(fileExtension)) // Validate file extension.
                {
                    ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                    return View(claim); // Return view with validation error.
                }

                // Create the upload directory if it doesn't exist.
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Save the file to the server.
                var filePath = Path.Combine(uploadsPath, document.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }

                // Save the file path for the claim.
                claim.SupportingDocumentPath = $"/uploads/{document.FileName}";
            }
            else
            {
                ModelState.AddModelError("document", "Please upload a supporting document.");
                return View(claim); // Return view with validation error.
            }

            // Assign the logged-in user's ID to the claim.
            claim.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Auto-decline claims that exceed criteria.
            if (claim.HourlyRate > 200 || claim.HoursWorked > 120)
            {
                claim.Status = "Declined";
                claim.DeclinedReason = "Hourly rate exceeds $200 or hours worked exceed 120.";
            }
            else
            {
                claim.Status = "Pending"; // Mark as pending for approval.
            }

            // Save the claim to the database.
            _dbContext.Claims.Add(claim);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ClaimSubmitted"); // Redirect to confirmation view.
        }

        // GET: Display a confirmation view after claim submission.
        public IActionResult ClaimSubmitted()
        {
            return View(); // Returns a confirmation view.
        }

        // GET: List all claims submitted by the logged-in lecturer.
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID.
                var userClaims = await _dbContext.Claims.Where(c => c.UserId == userId).ToListAsync(); // Fetch claims.
                return View(userClaims); // Pass claims to the view.
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching your claims. Please try again later.");
                Console.WriteLine(ex.Message); // Log the error.
                return View("Error"); // Return an error view.
            }
        }

        // GET: List all pending claims (restricted to Co-ordinators and Managers).
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpGet]
        public async Task<IActionResult> ViewPendingClaims()
        {
            try
            {
                var pendingClaims = await _dbContext.Claims.Where(c => c.Status == "Pending").ToListAsync(); // Fetch pending claims.
                return View(pendingClaims); // Pass pending claims to the view.
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
                Console.WriteLine(ex.Message); // Log the error.
                return View("Error"); // Return an error view.
            }
        }

        // POST: Approve a specific claim by its ID.
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                var claim = await _dbContext.Claims.FindAsync(id); // Find claim by ID.
                if (claim != null)
                {
                    claim.Status = "Approved"; // Update status to approved.
                    await _dbContext.SaveChangesAsync(); // Save changes.
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Claim not found.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while approving the claim. Please try again.");
                Console.WriteLine(ex.Message); // Log the error.
            }
            return RedirectToAction("ViewPendingClaims"); // Redirect to pending claims view.
        }

        // POST: Reject a specific claim by its ID.
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id)
        {
            try
            {
                var claim = await _dbContext.Claims.FindAsync(id); // Find claim by ID.
                if (claim != null)
                {
                    claim.Status = "Rejected"; // Update status to rejected.
                    await _dbContext.SaveChangesAsync(); // Save changes.
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Claim not found.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while rejecting the claim. Please try again.");
                Console.WriteLine(ex.Message); // Log the error.
            }
            return RedirectToAction("ViewPendingClaims"); // Redirect to pending claims view.
        }

        // POST: Delete a specific claim by its ID.
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _dbContext.Claims.FindAsync(id); // Find claim by ID.
            if (claim != null)
            {
                _dbContext.Claims.Remove(claim); // Remove the claim.
                await _dbContext.SaveChangesAsync(); // Save changes.
                return RedirectToAction("TrackClaims"); // Redirect to claims tracking view.
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found.");
                return View("Error"); // Return an error view.
            }
        }

        // GET: Generate a report of all claims.
        [HttpGet]
        public async Task<IActionResult> GenerateReport()
        {
            var claims = await _dbContext.Claims.ToListAsync(); // Fetch all claims.
            return View(claims); // Pass claims to the view.
        }

        // Generate and download a PDF report for approved claims.
        public IActionResult GeneratePdfReport()
        {
            try
            {
                // Fetch approved claims from the database.
                var claims = _dbContext.Claims.Where(c => c.Status == "Approved").ToList();

                // Create a temporary file path for the PDF.
                string filePath = Path.Combine(Path.GetTempPath(), "ApprovedClaimsReport.pdf");

                // Write report contents to the file.
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("Approved Claims Report");
                        writer.WriteLine($"Generated on: {DateTime.Now:g}");
                        writer.WriteLine(new string('-', 40));
                        writer.WriteLine($"{"Lecturer Name",-20} {"Hours Worked",-15} {"Hourly Rate",-15} {"Total Payment",-15}");
                        foreach (var claim in claims)
                        {
                            writer.WriteLine($"{claim.LecturerName,-20} {claim.HoursWorked,-15} {claim.HourlyRate,-15:C} {claim.FinalPayment,-15:C}");
                        }
                        writer.WriteLine(new string('-', 40));
                    }
                }

                // Return the PDF file for download.
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", "ApprovedClaimsReport.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                return View("Error");
            }
        }

        // Generate and download an Excel report for approved claims.
        public IActionResult GenerateExcelReport()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus.
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Approved Claims Report");

                    // Add headers.
                    worksheet.Cells[1, 1].Value = "Lecturer Name";
                    worksheet.Cells[1, 2].Value = "Hours Worked";
                    worksheet.Cells[1, 3].Value = "Hourly Rate";
                    worksheet.Cells[1, 4].Value = "Total Payment";

                    // Fetch approved claims.
                    var claims = _dbContext.Claims.Where(c => c.Status == "Approved").ToList();

                    // Populate rows with claim data.
                    int row = 2;
                    foreach (var claim in claims)
                    {
                        worksheet.Cells[row, 1].Value = claim.LecturerName;
                        worksheet.Cells[row, 2].Value = claim.HoursWorked;
                        worksheet.Cells[row, 3].Value = claim.HourlyRate;
                        worksheet.Cells[row, 4].Value = (double)claim.FinalPayment;
                        row++;
                    }

                    // Auto-fit columns for better readability.
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate Excel file as a byte array.
                    var excelData = package.GetAsByteArray();

                    // Return the file for download.
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApprovedClaimsReport.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating Excel: {ex.Message}");
                return View("Error");
            }
        }
    }
}

//# Assistance provided by ChatGPT
//# Code and support generated with the help of OpenAI's ChatGPT.
// code attribution
// W3schools
//https://www.w3schools.com/cs/index.php

// code attribution
//Bootswatch
//https://bootswatch.com/

// code attribution
// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-8.0&tabs=visual-studio

// code attribution
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio

// code attribution
// https://youtu.be/qvsWwwq2ynE?si=vwx2O4bCAFDFh5m_