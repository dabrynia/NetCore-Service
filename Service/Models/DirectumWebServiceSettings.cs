using Microsoft.Extensions.Configuration;

namespace Service.Models
{
    public class DirectumWebServiceSettings : SettingsBase
    {
        public DirectumWebServiceSettings(IConfiguration configuration, string sectionName = "DirectumWebService") : base(configuration, sectionName) { }

        public string Url => Section.GetValue<string>("Url");
    }
}
