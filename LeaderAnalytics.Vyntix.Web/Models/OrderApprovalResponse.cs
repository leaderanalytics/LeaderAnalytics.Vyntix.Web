using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Web.Models
{
    public class OrderApprovalResponse
    {
        public DateTime TimeStamp { get; private set; }
        public string ErrorMessage { get; set; }
        public string SessionID { get; set; }


        public OrderApprovalResponse()
        {
            TimeStamp = DateTime.UtcNow;
        }
    }
}
