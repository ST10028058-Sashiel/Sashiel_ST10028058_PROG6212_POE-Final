using System.ComponentModel.DataAnnotations;

namespace Sashiel_ST10028058_PROG6212_Part2.Models
{
    // The Claim class represents a claim submission made by a lecturer.
    // It includes attributes for tracking the claim details and associated metadata.
    public class Claim
    {
        // Primary key for the Claim entity.
        [Key]
        public int ClaimId { get; set; }

        // Stores the name of the lecturer submitting the claim.
        // This field is required and will be displayed as 'Lecturer' on the form.
        [Required]
        [Display(Name = "Lecturer")]
        public string LecturerName { get; set; }

        // Number of hours worked for the claim.
        // This field is required and must be a non-negative value.
        [Required]
        [Range(0, double.MaxValue)]
        public double HoursWorked { get; set; }

        // Hourly rate for the claim.
        // This field is required and must be a non-negative value.
        [Required]
        [Range(0, double.MaxValue)]
        public double HourlyRate { get; set; }

        // Calculate FinalPayment dynamically
        public decimal FinalPayment
        {
            get
            {
                return (decimal)(HoursWorked * HourlyRate);
            }
        }
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        // Status of the claim (e.g., 'Pending', 'Approved', or 'Rejected').
        // Default value is 'Pending'. This field is required.
        [Required]
        public string Status { get; set; } = "Pending";

        // Path to the supporting document uploaded for the claim.
        // This field is required and stores the location of the uploaded file.
        [Required]
        public string? SupportingDocumentPath { get; set; }

        public string UserId { get; set; }
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