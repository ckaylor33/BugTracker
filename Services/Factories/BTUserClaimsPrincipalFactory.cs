using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BugTracker.Services.Factories
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        //call on the parent to utilise the params in calling its' constructor - calling the parent to 
        //setup the class. No difference between the child and parent so we let the parent initialise
        //the class. Pass the params straight through and into the parent for initialisation of this class.
        public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {

        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            //allows us to create an identity based on BTUser passed in
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            //have a hold of the user passed in (logged into the app) - can now add claim for that person
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}
