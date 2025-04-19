namespace AppRepository.Connect
{
    public class ConnectionStringOption
    {
        public const string Key = "ConnectionStrings";
        public string DefaultConnection { get; set; } = default!;
    }
}
