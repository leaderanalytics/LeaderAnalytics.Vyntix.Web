using LeaderAnalytics.Vyntix.Web.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace LeaderAnalytics.Vyntix.Web
{
    public class SessionCache
    {
        private ConcurrentDictionary<string, OrderSession> Sessions { get; set; }
        private readonly int lifetimeMinutes;

        public SessionCache(int lifetimeMinutes = 120)
        {
            this.lifetimeMinutes = lifetimeMinutes;
            Sessions = new ConcurrentDictionary<string, OrderSession>();
        }

        public bool AddSession(OrderSession oar)
        {
            bool result = Sessions.TryAdd(oar.SessionID, oar);

            if (result)
                Log.Information("AddSession: Session {s} was added to SessionCache.", oar.SessionID);
            else
                Log.Error("AddSession: Session {s} could not be added to SessionCache.", oar.SessionID);

            return result;
        }

        public List<OrderSession> GetSessions()
        {
            ClearExpiredSessions();
            return Sessions.Values.ToList();
        }

        public OrderSession GetSession(string sessionID)
        {
            ClearExpiredSessions();
            Sessions.Remove(sessionID, out OrderSession os);

            if (os != null)
                Log.Information("GetSession: Session {s} was removed from SessionCache.", sessionID);
            else
                Log.Error("GetSession: Session {s} was not found in SessionCache.", sessionID);

            return os;
        }

        public void ClearExpiredSessions()
        {
            DateTime now = DateTime.UtcNow;
            List<string> expired = Sessions.Where(x => x.Value.TimeStamp.AddMinutes(lifetimeMinutes) < now).Select(x => x.Key).ToList();

            foreach (string s in expired)
            {
                Sessions.Remove(s, out OrderSession v);
                Log.Information("ClearExpiredSessions: Session {s} expired and was removed from SessionCache.", s);
            }
        }
    }
}
