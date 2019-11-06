using Microsoft.Extensions.Configuration;

namespace Service.Models
{
    public class Settings : SettingsBase
    {
        public Settings(IConfiguration configuration, string sectionName = "main") : base(configuration, sectionName) { }

        public int WorkersCount => Section.GetValue<int>("WorkersCount");
        public int RunInterval => Section.GetValue<int>("RunInterval");
        public string InstanceName => Section.GetValue<string>("name");
        public string ResultPath => Section.GetValue<string>("ResultPath");
    }
}
