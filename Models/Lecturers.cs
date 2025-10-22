using System.ComponentModel.DataAnnotations;

namespace PROGA22025.Models
{
    public class Lecturers
    {
        public int LecturerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
