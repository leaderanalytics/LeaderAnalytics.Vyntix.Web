using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Domain
{

    public class ConfigFilePathParameter : IValueParameter<string>
    {
        public string Value { get; set; }
    }
}
