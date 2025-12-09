using System.Net;

namespace AssetRegistry.Exceptions
{
    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException(HttpStatusCode code) : base($"{code}")
        {
        }
    }

}
