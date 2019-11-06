using Microsoft.Extensions.Configuration;

namespace Service.Models
{
    public class RMQConnectionSettings : SettingsBase
    {
        public RMQConnectionSettings(IConfiguration configuration, string sectionName = "RMQConnetion") : base(configuration, sectionName) { }

        public string UserName => Section.GetValue<string>("UserName");
        public string Password => Section.GetValue<string>("Password");
        public string HostName => Section.GetValue<string>("HostName");
        public int Port => Section.GetValue<int>("Port");
    }
}
