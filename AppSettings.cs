namespace InfraLib
{
    public class AppSettings
    {
        public static string[] AllowedOrigins { get; set; }

        public static string ServicesAPIURL { get; set; }

        public static string ServicesWWWRoot { get; set; }

        public static string EnvironmentName { get; set; }

        public static string ApplicationName { get; set; }

        public static string SeqURL { get; set; }

        public static DBSettings dbSettings { get; set; }

        public static HangfireSettings hangfireSettings { get; set; }
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
