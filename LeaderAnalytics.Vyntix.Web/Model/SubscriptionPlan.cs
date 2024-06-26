﻿namespace LeaderAnalytics.Vyntix.Web.Model;

public class SubscriptionPlan : ITemporal
{
    public DateTime? StartDate { get; set; }            // The date the plan can first be subscribed too. 
    public DateTime? EndDate { get; set; }              // The date after which the plan can no longer be subscribed too.
    public string PaymentProviderPlanID { get; set; }   // ID used by a payment provider i.e. Stripe price ID.
    public string PlanDescription { get; set; }         // Complete description of the plan and it's terms.
    public string ShortDescription { get; set; }
    public decimal Cost { get; set; }                   // The amount charged to the customers account each billing period.
    public int BillingPeriods { get; set; }             // The number of times the customer is charged each year.  12 = monthly, 2 = every six months, etc.
    public int DisplaySequence { get; set; }            // Ordinal position.
    public decimal MonthlyCost => (Cost / 12) * BillingPeriods;
}
