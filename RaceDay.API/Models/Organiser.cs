using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Organiser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrganiserID { get; set; }

        [Column("fullName")]
        public string Name { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("createdAT")]
        public DateTime? CreatedAT { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}
