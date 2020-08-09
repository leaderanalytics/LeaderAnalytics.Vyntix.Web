"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var SubscriptionPlan = /** @class */ (function () {
    function SubscriptionPlan() {
        this.PaymentProviderPlanID = ""; // ID used by a payment provider i.e. Stripe price ID.
        this.PlanDescription = ""; // Complete description of the plan and it's terms.
        this.Cost = 0; // The amount charged to the customers account each billing period.
        this.BillingPeriods = 2; // The number of times the customer is charged each year.  12 = monthly, 2 = every six months, etc. 
        this.DisplaySequence = 0; // Ordinal position. 
    }
    Object.defineProperty(SubscriptionPlan.prototype, "MonthlyCost", {
        get: function () { return (this.Cost / 12) * this.BillingPeriods; },
        enumerable: true,
        configurable: true
    });
    return SubscriptionPlan;
}());
exports.default = SubscriptionPlan;
;
//# sourceMappingURL=SubscriptionPlan.js.map