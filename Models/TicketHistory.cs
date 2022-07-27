using System;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; } //carries the id of a ticket that exists in another table (foreign key)

        [DisplayName("Updated Item")]
        public string Property { get; set; } //represents property of a ticket that may be modified - example:
        //typo in description of a ticket (ticket has description property) this needs to be edited and saved 
        //back into the db - this will create a history item. Need that history item to tell me which property
        //of a ticket was modified.

        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description of Change")]
        public string Description { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; } //this is used to navigate to the BTUser (foreign key)

        //NAVIGATION PROPERTIES//
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
