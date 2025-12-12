using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    [Table("Contract")]
    public class Contract
    {
        [Key]
        [Column("contract_id")]
        public int ContractId { get; set; }

        [Column("driver_id")]
        public int DriverId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("season_id")]
        public int SeasonId { get; set; }

        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }

        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        [ForeignKey("SeasonId")]
        public Season Season { get; set; }
    }
}
