using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sashiel_ST10028058_PROG6212_Part2.Data;
using Sashiel_ST10028058_PROG6212_Part2.Models;

namespace Sashiel_ST10028058_PROG6212_Part2.Controllers
{
    // Controller for managing claims-related operations
    public class ClaimsController : Controller
    {
        // Injected ApplicationDbContext for database access
        private readonly ApplicationDbContext _dbContext;

        // Max file size limit (5 MB) and allowed file extensions for uploads
        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" };

        // Constructor to inject the database context
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
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile document)
        {
            // Check if a file was uploaded and is not empty
            if (document != null && document.Length > 0)
            {
                // Validate file extension
                var fileExtension = Path.GetExtension(document.FileName).ToLower();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                    return View(claim);
                }

                // Define the path for file uploads
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath); // Create the directory if it doesn't exist
                }

                // Save the uploaded file to the server
                var filePath = Path.Combine(uploadsPath, document.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream); // Copy the uploaded file to the destination
                }

                // Store the file path in the claim object
                claim.SupportingDocumentPath = $"/uploads/{document.FileName}";
            }
            else
            {
                // Add a model error if no document was uploaded
                ModelState.AddModelError("document", "Please upload a supporting document.");
                return View(claim);
            }

            // Add the claim to the database and save changes
            _dbContext.Claims.Add(claim);
            await _dbContext.SaveChangesAsync();

            // Redirect to the ClaimSubmitted view
            return RedirectToAction("ClaimSubmitted");
        }

        // GET: Display a confirmation view after claim submission
        public IActionResult ClaimSubmitted()
        {
            return View();
        }

        // GET: View all pending claims (restricted to Co-ordinators and Managers)
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpGet]
        public async Task<IActionResult> ViewPendingClaims()
        {
            try
            {
                // Retrieve pending claims from the database
                var pendingClaims = await _dbContext.Claims.Where(c => c.Status == "Pending").ToListAsync();
                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                // Handle any errors during data retrieval
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // POST: Approve a specific claim (restricted to Co-ordinators and Managers)
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                // Find the claim by its ID
                var claim = await _dbContext.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Approved"; // Update the claim status
                    await _dbContext.SaveChangesAsync(); // Save changes to the database
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

        // POST: Reject a specific claim (restricted to Co-ordinators and Managers)
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id)
        {
            try
            {
                var claim = await _dbContext.Claims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = "Rejected"; // Update the claim status
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

        // GET: Track all claims (restricted to authenticated users)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                // Retrieve all claims from the database
                var allClaims = await _dbContext.Claims.ToListAsync();
                return View(allClaims);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
                Console.WriteLine(ex.Message);
                return View("Error");
            }
        }

        // POST: Delete a specific claim (restricted to Co-ordinators and Managers)
        [Authorize(Roles = "Co-ordinator,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _dbContext.Claims.FindAsync(id);
            if (claim != null)
            {
                _dbContext.Claims.Remove(claim); // Remove the claim from the database
                await _dbContext.SaveChangesAsync(); // Save changes
                return RedirectToAction("TrackClaims");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found.");
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