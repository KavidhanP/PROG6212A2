using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROGA22025.Models
{

    public class ClaimViewModel
    {
        public int LecturerId { get; set; }

        [Required]
        [Range(1, 500)]
        public int HoursWorked { get; set; }

        [Required]
        [Range(1, 5000)]
        public decimal HourlyRate { get; set; }

        public string Notes { get; set; }

        public List<IFormFile> Documents { get; set; }
    }
}
