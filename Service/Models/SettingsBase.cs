using Microsoft.Extensions.Configuration;

namespace Service.Models
{
    public class SettingsBase
    {
        public SettingsBase(IConfiguration configuration, string sectionName)
        {
            this.Section = configuration.GetSection(sectionName);
        }

        protected IConfigurationSection Section { get; }
    }
}
