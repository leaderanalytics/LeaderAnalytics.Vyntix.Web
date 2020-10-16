export default class SubscriptionPlan {
    public StartDate?: Date;                            // The date the plan can first be subscribed too. 
    public EndDate?: Date;                              // The date after which the plan can no longer be subscribed too. 
    public PaymentProviderPlanID: string = "";         // ID used by a payment provider i.e. Stripe price ID.
    public PlanDescription: string = "";               // Complete description of the plan and it's terms.
    public ShortDescription: string = "";
    public Cost: number = 0;                           // The amount charged to the customers account each billing period.
    public BillingPeriods: number = 2;                 // The number of times the customer is charged each year.  12 = monthly, 2 = every six months, etc. 
    public DisplaySequence: number = 0;                // Ordinal position. 
    public get MonthlyCost(): number { return (this.Cost / 12) * this.BillingPeriods }

    public static Create(s: SubscriptionPlan | null): SubscriptionPlan | null {
        const p: SubscriptionPlan = new SubscriptionPlan();

        if (s === null)
            return null;

        p.StartDate = s.StartDate;
        p.EndDate = s.EndDate;
        p.PaymentProviderPlanID = s.PaymentProviderPlanID;
        p.PlanDescription = s.PlanDescription;
        p.ShortDescription = s.ShortDescription;
        p.Cost = s.Cost;
        p.BillingPeriods = s.BillingPeriods;
        p.DisplaySequence = s.DisplaySequence;
        return p;
    }
}
