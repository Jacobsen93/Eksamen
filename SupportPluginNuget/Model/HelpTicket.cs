using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SupportPluginNuget.Model
{
    public class HelpTicket : Ticket
    {
        [Required]
        [DisplayName("Issue Description")]
        public string IssueDescription { get; set; }

        [Required]
        public HelpTicketSubject Subject { get; set; }
    }
}
