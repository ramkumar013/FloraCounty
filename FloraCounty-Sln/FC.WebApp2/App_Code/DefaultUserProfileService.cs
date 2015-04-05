using ExtendedMongoMembership;
using ExtendedMongoMembership.Services;

namespace FC.WebApp.AppCode
{
    public class DefaultUserProfileService : UserProfileServiceBase<FCMember>
    {
        public DefaultUserProfileService(string connectionString)
            : base(connectionString)
        {

        }
    }
}