using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    //IdentityUser will extend to BTUser - class that's built into micrsoft. String as primary key, also inherits
    //from IUClaim, IURole, IULogin (all of this is built in to the class). Gives ability to cutomise BTUser with 
    //additional info not there by default - phone number, email, etc
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        //get and set allow us to either retrieve the value stored or set the value for the public property

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        //when someone wants the value, we tell FullName how to get it - will concatinate FirstName and LastName
    }
}
