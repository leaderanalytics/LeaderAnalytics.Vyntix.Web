export default class SubscriptionPlan {
    public StartDate?: Date;                            // The date the plan can first be subscribed too. 
    public EndDate?: Date;                              // The date after which the plan can no longer be subscribed too. 
    public PlanPaymentProviderID: string = "";         // ID used by a payment provider i.e. Stripe price ID.
    public PlanDescription: string = "";               // Complete description of the plan and it's terms.
    public Cost: number = 0;                           // The amount charged to the customers account each billing period.
    public BillingPeriods: number = 2;                 // The number of times the customer is charged each year.  12 = monthly, 2 = every six months, etc. 
    public DisplaySequence: number = 0;                // Ordinal position. 
    public get MonthlyCost(): number { return (this.Cost / 12) * this.BillingPeriods }
};
