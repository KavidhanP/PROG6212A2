using System.ComponentModel.DataAnnotations;

namespace PROGA22025.Models
{
    public class Claims
    {
        public int ClaimId { get; set; }
        public int LecturerId { get; set; }

        [Required]
        [Range(1, 500)]
        public int HoursWorked { get; set; }

        [Required]
        [Range(1, 5000)]
        public decimal HourlyRate { get; set; }

        public string Notes { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        public bool? CoordinatorApproved { get; set; }
        public bool? ManagerApproved { get; set; }

        
        public List<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();

        public decimal Total => HoursWorked * HourlyRate;
    }
}
