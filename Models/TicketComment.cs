using System;
using System.ComponentModel;

namespace BugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Member Comment")]
        public string Comment { get; set; }

        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        //NAVIGATION PROPERTIES// - used to look in the ticket table for the record of the ticket and gives
        //access to the ticket properties (date, description, user, etc) - reference or address of the ticket

        public virtual Ticket Ticket { get; set; }

        public virtual BTUser User { get; set; } //not the id of the user, it is the user itself - therefore
        //need to give it the type that the user is (BTUser)
    }
}
