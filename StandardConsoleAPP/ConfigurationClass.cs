namespace StandardConsoleApp
{
    public class ConfigurationClass
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string AreaName { get; set; }
        public string Host { get; set; }
        public string ApiUser { get; set; }
        public string ApiPass { get; set; }
        public string[] ExcludedInterfaces { get; set; }

    }
}
