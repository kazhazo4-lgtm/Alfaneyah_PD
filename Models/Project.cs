using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDashboards.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(200)]
        [Display(Name = "Project Name")]
        public string? ProjectName { get; set; }

        [Display(Name = "Project Location")]
        [StringLength(200)]
        public string ProjectLocation { get; set; }

        [Display(Name = "Scope of Works")]
        public string? Scope { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Contract Start Date")]
        [DataType(DataType.Date)]
        public DateTime? ContractStartDate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Contract End Date")]
        [DataType(DataType.Date)]
        public DateTime? ContractEndDate { get; set; }

        // ENCRYPTED: Database stores encrypted value
        [Display(Name = "Contract Value")]
        public string? EncryptedContractValue { get; set; }

        // For display/editing (not stored in database)
        [NotMapped]
        [Display(Name = "Contract Value")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal? ContractValue
        {
            get => _contractValue;
            set => _contractValue = value;
        }
        private decimal? _contractValue;

        [ForeignKey("CreatedByUser")]
        public int? CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }
        public ICollection<PaymentClaim>? PaymentClaims { get; set; }
        public ICollection<VariationOrder>? VariationOrders { get; set; }

        [Display(Name = "Revised End Date")]
        [DataType(DataType.Date)]
        public DateTime? RevisedEndDate { get; set; }

        // Computed properties
        [NotMapped]
        [Display(Name = "Duration (Days)")]
        public int? Duration
        {
            get
            {
                if (ContractStartDate.HasValue && ContractEndDate.HasValue)
                {
                    return (ContractEndDate.Value - ContractStartDate.Value).Days;
                }
                return null;
            }
        }

        [NotMapped]
        [Display(Name = "Total VOs")]
        public decimal? TotalVOs => VariationOrders?.Sum(v => v.VOAmount) ?? 0;

        [NotMapped]
        [Display(Name = "Revised Contract Value")]
        public decimal? RevisedContractValue => (ContractValue ?? 0) + TotalVOs;

        [NotMapped]
        [Display(Name = "Total Claims")]
        public decimal? TotalClaims => PaymentClaims?.Sum(p => p.ClaimAmount) ?? 0;

        [NotMapped]
        [Display(Name = "Progress %")]
        public decimal? ProgressPercentage
        {
            get
            {
                if (RevisedContractValue > 0)
                {
                    return Math.Round((TotalClaims / RevisedContractValue * 100) ?? 0, 2);
                }
                return 0;
            }
        }

        [NotMapped]
        [Display(Name = "Status")]
        public string Status
        {
            get
            {
                if (ProgressPercentage >= 100)
                    return "Completed";
                else if (ProgressPercentage > 0)
                    return "In Progress";
                else
                    return "Not Started";
            }
        }
    }
}
