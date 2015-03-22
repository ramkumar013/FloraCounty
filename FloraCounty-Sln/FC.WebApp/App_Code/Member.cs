using ExtendedMongoMembership;
using ExtendedMongoMembership.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FC.WebApp
{
    public class FCMember : MembershipAccount
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}