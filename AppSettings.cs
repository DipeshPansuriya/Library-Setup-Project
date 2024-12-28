namespace InfraLib
{
    public class AppSettings
    {
        public string[] AllowedOrigins { get; set; }

        public string ServicesAPIURL { get; set; }

        public string ServicesWWWRoot { get; set; }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; }

        public string SeqURL { get; set; }

        public DBSettings dbSettings { get; set; }

        public HangfireSettings hangfireSettings { get; set; }
    }

    public class DBSettings
    {
        public string MastersDb { get; set; }

        public string MastersDbName { get; set; }

        public string MastersLogDb { get; set; }

        public string MastersLogDbName { get; set; }

        public string MastersHangfireDb { get; set; }

        public string MastersHangfireDbName { get; set; }
    }

    public class HangfireSettings
    {
        public string ServicesHangfireURL { get; set; }

        public string Accessid { get; set; }

        public string Accesspwd { get; set; }
    }
}
