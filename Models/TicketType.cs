using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

//classes in OOP are the base of object code writing - have to have a class that defines an object
//or to have an object we have to have a class that's given its' blueprint
namespace BugTracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [DisplayName("Type Name")]
        public string Name { get; set; }
    }
}
