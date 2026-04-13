using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDashboards.Models
{
    [Table("LoginAttempts")]
    public class LoginAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? EmailOrName { get; set; }

        [StringLength(100)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime AttemptTime { get; set; }

        [Required]
        [StringLength(20)]
        public string? Status { get; set; } // Success, Failed, Blocked, Approved, Pending

        [StringLength(1000)]
        public string? FailureReason { get; set; }

        public int? AttemptCount { get; set; }

        // If this attempt was later approved to become a user
        public int? ApprovedUserId { get; set; }

        [StringLength(50)]
        public string? FlaggedAs { get; set; } // Suspicious, Unauthorized, HackingAttempt

        [ForeignKey("ApprovedUserId")]
        public User? ApprovedUser { get; set; }

        public bool IsBlocked { get; set; } = false;

        public DateTime? BlockedUntil { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    [Table("BlockedVisitors")]
    public class BlockedVisitor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? EmailOrName { get; set; }

        [StringLength(100)]
        public string? IPAddress { get; set; }

        public DateTime BlockedAt { get; set; }

        public int BlockedByUserId { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        public DateTime? BlockedUntil { get; set; }

        public bool IsPermanent { get; set; } = true;

        [ForeignKey("BlockedByUserId")]
        public User? BlockedByUser { get; set; }
    }
}