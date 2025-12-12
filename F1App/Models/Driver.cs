using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Driver
    {
        [Key]
        [Column("driver_id")]
        public int DriverId { get; set; }

        [Column("driver_number")]
        public int DriverNumber { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("nationality")]
        public string? Nationality { get; set; }

        [Column("headshot_url")]
        public string? HeadshotUrl { get; set; }
    }
}
