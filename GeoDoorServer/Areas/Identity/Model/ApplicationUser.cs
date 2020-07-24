using System;
using Microsoft.AspNetCore.Identity;

namespace GeoDoorServer.Areas.Identity.Model
{
    public class ApplicationUser : IdentityUser
    {
        public AccessRights AccessRights { get; set; }
        public DateTime LastConnection { get; set; }
    }
    
    public enum AccessRights
    {
        Allowed,
        NotAllowed,
        Register
    }
}