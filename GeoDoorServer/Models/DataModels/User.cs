using System;
using System.Xml;
using GeoDoorServer.Areas.Identity.Model;

namespace GeoDoorServer.Models.DataModels
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneId { get; set; }
        public string Name { get; set; }
        public AccessRights AccessRights { get; set; }
        public DateTime LastConnection { get; set; }
    }
}
