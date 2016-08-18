namespace Dovetail.SDK.Clarify
{
    public class DovetailDatabaseSettings
    {
        public DovetailDatabaseSettings()
        {
            ApplicationUsername = "sa";
        }

        public string Type { get; set; }
        public string ConnectionString { get; set; }
        public double SessionTimeoutInMinutes { get; set; }
        public string ApplicationUsername { get; set; }
    }

    public class DovetailCRMSettings
    {
        public string DatabaseType { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string EmployeeUserName { get; set; }
    }
}