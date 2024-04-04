namespace cCoder.Core.Objects.Dtos
{
    public class MigrateOptions
    {
        public string Domain { get; set; }
        public string Password { get; set; }
        public string[] PackageNames { get; set; }
    }
}