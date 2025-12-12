using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1App.Models
{
    public class Session
    {
        [Key]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("event_id")]
        public int EventId { get; set; }

        [Column("session_name")]
        public string SessionName { get; set; }

        [Column("session_type")]
        public string SessionType { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }
    }
}
