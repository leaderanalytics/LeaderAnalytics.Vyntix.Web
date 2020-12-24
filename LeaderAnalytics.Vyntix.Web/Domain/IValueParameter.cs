using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Domain
{
    public interface IValueParameter<T> 
    {
        T Value { get; set; }
    }
}
