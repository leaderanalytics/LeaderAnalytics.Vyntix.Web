var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import { loadStripe } from '@stripe/stripe-js';
// ----------------------------------------
// https://github.com/stripe/stripe-js
// https://github.com/stripe/stripe-node#usage-with-typescript
// https://github.com/stripe-samples/accept-a-card-payment/tree/master/using-webhooks/server/node-typescript
// ----------------------------------------
export default class Subsribe {
    constructor(stripe_pk) {
        this.stripe_pk = stripe_pk;
    }
    NavigateToPaymentProcessor(isLoggedIn) {
        return __awaiter(this, void 0, void 0, function* () {
            const c = this.getSelectionCount();
            alert('hello this is NavigateToPaymentProcessor');
            if (c === 0) {
                alert("Please choose a subscription.");
                return;
            }
            else if (c > 1) {
                alert("Please choose only one subscription.");
                return;
            }
            const product = this.getSelectedSubscription();
            $.post('/subscription/accountsetup');
            //const stripe = await loadStripe(this.stripe_pk);
            const stripe = yield loadStripe(this.stripe_pk);
            stripe.redirectToCheckout({
                customerEmail: "sam.wheat@outlook.com",
                lineItems: [{ price: product, quantity: 1 }],
                successUrl: '/Subscription',
                cancelUrl: '/Subscription',
                mode: 'subscription'
            });
        });
    }
    getSelectionCount() {
        return $("#selectionParent > input:checked").length;
    }
    getSelectedSubscription() {
        return $("#selectionParent > input:checked").attr("data");
    }
    handleResult(result) {
        if (result.error) {
            const displayError = document.getElementById("error-message");
            displayError.textContent = result.error.message;
        }
    }
}
//# sourceMappingURL=Subscribe.js.map