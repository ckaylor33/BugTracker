using System.Security.Claims;
using System.Security.Principal;

namespace BugTracker.Extensions
{
    public static class IdentityExtensions
    {
        //can capture any class that implements IIdentity - principal identity gets extended by this method
        //allows this method to be accessible from the principal
        public static int? GetCompanyId(this IIdentity identity)
        {
            //captures the info gathered from login (principal factory) - call upon it
            //ClaimsIdentity implements the IIdentity
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");
            //Ternary Operator (if/else)
            return (claim != null) ? int.Parse(claim.Value) : null;
        }
    }
}
