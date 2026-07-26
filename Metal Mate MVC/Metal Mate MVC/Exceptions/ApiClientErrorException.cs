namespace Metal_Mate_MVC.Exceptions
{
    public class ApiClientErrorException : Exception
    {
        public ApiClientErrorException(string message)
            : base(message)
        {
        }
    }
}
