using ExtendedMongoMembership;
using ExtendedMongoMembership.Services;

namespace FC.WebApp
{
    public class DefaultUserProfileService : UserProfileServiceBase<FCMember>
    {
        public DefaultUserProfileService(string connectionString)
            : base(connectionString)
        {

        }
    }
}