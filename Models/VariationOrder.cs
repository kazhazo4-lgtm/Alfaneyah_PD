using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDashboards.Models
{
    [Table("VariationOrders")]
    public class VariationOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Scope of Work")]
        [StringLength(500)]
        [Required(ErrorMessage = "Scope of Work is required")] // Add this line to make it required
        public string Scope { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "VO Amount")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal VOAmount { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [Display(Name = "Approved Date")]
        [DataType(DataType.Date)]
        public DateTime ApprovedDate { get; set; }

        // Navigation property
        public Project? Project { get; set; }
    }
}