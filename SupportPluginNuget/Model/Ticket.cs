using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportPluginNuget.Model
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        [StringLength(50)]
        public string Title { get; set; }

        [DisplayName("Besked")]
        public string Message { get; set; }

        [DisplayName("Application ID")]
        public string ApplicationId { get; set; }

        [DisplayName("User ID")]
        public string UserId { get; set; }
    }
}
