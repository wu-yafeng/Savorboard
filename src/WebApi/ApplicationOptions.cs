namespace WebApi
{
    public class ApplicationOptions
    {
        public required string JsonWebKey { get; set; }

        public required IEnumerable<string> Issuers { get; set; }

        public required IEnumerable<long> Servers { get; set; }
    }
}
