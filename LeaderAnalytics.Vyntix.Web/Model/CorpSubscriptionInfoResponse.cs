﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.Vyntix.Web.Domain;

namespace LeaderAnalytics.Vyntix.Web.Model
{
    public class CorpSubscriptionInfoResponse: AsyncResult
    {
        public string AdminEmail { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
