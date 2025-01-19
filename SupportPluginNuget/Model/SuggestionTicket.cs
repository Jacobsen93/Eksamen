using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SupportPluginNuget.Model
{
    public class SuggestionTicket : Ticket
    {
        [Required]
        [DisplayName("Suggestion Details")]
        public string SuggestionDetails { get; set; }

        [Required]
        public SuggestionTicketSubject Subject { get; set; }
    }
}
