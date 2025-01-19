using Microsoft.AspNetCore.Mvc;
using SupportPluginNuget.Model;


namespace SupportPluginApi.Controllers
{
  
    public class SupportPluginController : ControllerBase
    {

        [HttpGet("GetSupportInfo")]
        public IActionResult GetSupportInfo()
        {
            return Ok("Support information from plugin api");
        }

        [HttpPost("CreateTicket")]
        public async Task<ActionResult<Ticket>> CreateTicket([FromBody]Ticket ticket)
        {

        
            return Ok("ticket oprettet");
            
        }
    }
}
