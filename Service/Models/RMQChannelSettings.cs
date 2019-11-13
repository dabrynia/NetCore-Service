using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Models
{
    public class RMQChannelSettings : SettingsBase
    {
        public RMQChannelSettings(IConfiguration configuration, string sectionName = "RMQChannel") : base(configuration, sectionName) { }

        public string QueueName => Section.GetValue<string>("QueueName");
        public string ExchangeName => Section.GetValue<string>("ExchangeName");
        public string RoutingKey => Section.GetValue<string>("RoutingKey");
        public string Type => Section.GetValue<string>("Type");
        public bool Durable => Section.GetValue<bool>("Durable");
    }
}
