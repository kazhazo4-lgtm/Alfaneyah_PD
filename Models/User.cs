using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDashboards.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        // Navigation property
        public ICollection<Project>? CreatedProjects { get; set; }
    }
}
