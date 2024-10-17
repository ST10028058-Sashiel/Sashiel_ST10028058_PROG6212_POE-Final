using System.ComponentModel.DataAnnotations;

namespace Sashiel_ST10028058_PROG6212_Part2.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [Display(Name = "Lecturer")]
        public string LecturerName { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double HoursWorked { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double HourlyRate { get; set; }

        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public string? SupportingDocumentPath { get; set; }
    }
}
