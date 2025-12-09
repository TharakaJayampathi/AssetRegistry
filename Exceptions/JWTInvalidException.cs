using System.Net;

namespace AssetRegistry.Exceptions
{
    public class JWTInvalidException : Exception
    {
        public JWTInvalidException(HttpStatusCode code, string error) : base($"{error}")
        {
        }
    }
}
