namespace TaxiWeb.Models
{
    public class Config
    {
        public object Logging { get;set; }
        public string AllowedHosts { get; set; }
        public JWTConfig JWT { get; set; } 

    }

    public class JWTConfig
    {
        public string Secret { get; set; }
    }
}
