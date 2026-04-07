using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDashboards.Models
{
    [Table("PaymentClaims")]
    public class PaymentClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Claim Amount")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal ClaimAmount { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [Display(Name = "SUBTD Date")]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; }

        // New column: Month Payment
        [Display(Name = "Month Payment")]
        [StringLength(50, ErrorMessage = "Month cannot exceed 50 characters")]
        public string? MonthPayment { get; set; }

        // New column: Approved Date
        [Display(Name = "APVD Date")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? ApprovedDate { get; set; }

        // New column: VAT Date
        [Display(Name = "VAT Date")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? VATDate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        // Navigation property
        public Project? Project { get; set; }
    }
}