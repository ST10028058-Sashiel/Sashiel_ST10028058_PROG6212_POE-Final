using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sashiel_ST10028058_PROG6212_Part2.Data;
using AppClaim = Sashiel_ST10028058_PROG6212_Part2.Models.Claim;
using OfficeOpenXml;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;


namespace Sashiel_ST10028058_PROG6212_Part2.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" };

        public ClaimsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Display the claim submission form (restricted to Lecturers)
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View();
        }

        // POST: Handle the submission of a new claim with a supporting document
        [HttpPost]
        public async Task<IActionResult> SubmitClaim(AppClaim claim, IFormFile document)
        {
            if (document != null && document.Length > 0)
            {
                var fileExtension = Path.GetExtension(document.FileName).ToLower();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                    return View(claim);
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var filePath = Path.Combine(uploadsPath, document.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }

                claim.SupportingDocumentPath = $"/uploads/{document.FileName}";
            }
            else
            {
                ModelState.AddModelError("document", "Please upload a supporting document.");
                return View(claim);
            }

            // Set the UserId for the logged-in user
            claim.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _dbContext.Claims.Add(claim);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ClaimSubmitted");
        }

        // GET: Display a confirmation view after claim submission
        public IActionResult ClaimSubmitted()
        {
            return View();
        }

        // GET: Track all claims for the logged-in lecturer
        [Authorize(Roles = "Lecturer")]
        [HttpGet]
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userClaims = await _dbContext.Claims.Where(c => c.UserId == userId).ToListAsync();
                return View(userClaims);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching your claims. Please try again later.");
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // GET: View all pending claims (restricted to Co-ordinators and Managers)
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpGet]
        public async Task<IActionResult> ViewPendingClaims()
        {
            try
            {
                var pendingClaims = await _dbContext.Claims.Where(c => c.Status == "Pending").ToListAsync();
                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // POST: Approve a specific claim
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                var claim = await _dbContext.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Approved";
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Claim not found.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while approving the claim. Please try again.");
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("ViewPendingClaims");
        }

        // POST: Reject a specific claim
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id)
        {
            try
            {
                var claim = await _dbContext.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Rejected";
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Claim not found.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while rejecting the claim. Please try again.");
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("ViewPendingClaims");
        }

        // POST: Delete a specific claim
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _dbContext.Claims.FindAsync(id);
            if (claim != null)
            {
                _dbContext.Claims.Remove(claim);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("TrackClaims");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found.");
                return View("Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GenerateReport()
        {
            var claims = await _dbContext.Claims.ToListAsync();
            return View(claims);
        }

        public IActionResult GeneratePdfReport()
        {
            try
            {
                // Fetch approved claims from the database
                var claims = _dbContext.Claims.Where(c => c.Status == "Approved").ToList();

                // Create a temporary file path for the PDF
                string filePath = Path.Combine(Path.GetTempPath(), "ApprovedClaimsReport.pdf");

                // Write report contents
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("Approved Claims Report");
                        writer.WriteLine($"Generated on: {DateTime.Now:g}");
                        writer.WriteLine(new string('-', 40));

                        // Add headers
                        writer.WriteLine($"{"Lecturer Name",-20} {"Hours Worked",-15} {"Hourly Rate",-15} {"Total Payment",-15}");

                        // Add claim details
                        foreach (var claim in claims)
                        {
                            writer.WriteLine($"{claim.LecturerName,-20} {claim.HoursWorked,-15} {claim.HourlyRate,-15:C} {claim.FinalPayment,-15:C}");
                        }

                        writer.WriteLine(new string('-', 40));
                    }
                }

                // Return the PDF file for download
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", "ApprovedClaimsReport.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                return View("Error");
            }
        }


        public IActionResult GenerateExcelReport()
        {
            try
            {
                // Set the license context for EPPlus
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Approved Claims Report");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Lecturer Name";
                    worksheet.Cells[1, 2].Value = "Hours Worked";
                    worksheet.Cells[1, 3].Value = "Hourly Rate";
                    worksheet.Cells[1, 4].Value = "Total Payment";

                    // Fetch approved claims
                    var claims = _dbContext.Claims.Where(c => c.Status == "Approved").ToList();

                    // Add data rows
                    int row = 2;
                    foreach (var claim in claims)
                    {
                        worksheet.Cells[row, 1].Value = claim.LecturerName;
                        worksheet.Cells[row, 2].Value = claim.HoursWorked;
                        worksheet.Cells[row, 3].Value = claim.HourlyRate;
                        worksheet.Cells[row, 4].Value = (double)claim.FinalPayment;
                        row++;
                    }

                    // Auto-fit columns for better formatting
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generate the Excel file in memory
                    var excelData = package.GetAsByteArray();

                    // Return the file for download
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
