using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
namespace PROGA22025.Models
{
 

    public class SupportingDocument
    {
        public int DocumentId { get; set; }

        public int ClaimId { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; }

        public long FileSize { get; set; }

        [StringLength(10)]
        public string FileType { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}
