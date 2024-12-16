using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DiscussedApi.Authenctication
{
    public class BuildTokenException : Exception
    {
        public BuildTokenException() 
            : base("An Exception occured while building authentication tokens")
        { 
        }

        public BuildTokenException(string message) : base(message)
        {
        }

        public BuildTokenException(string message, Exception innerException) 
            :base(message, innerException)
        {

        }
    }
}
