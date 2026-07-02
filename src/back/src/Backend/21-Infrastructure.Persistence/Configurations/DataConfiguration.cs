namespace Infrastructure.Persistence.Configurations
{
    public class DataConfiguration
    {
        public string DefaultUserPassword { get; set; }
        public bool Seed { get; set; } = false;
        public bool SeedTest { get; set; } = false;
    }
}
