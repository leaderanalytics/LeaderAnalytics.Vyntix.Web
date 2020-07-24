// ----------------------------------------
// https://github.com/stripe/stripe-js
// ----------------------------------------
import { loadStripe } from '../node_modules/@stripe/stripe-js/pure';
export default class Subsribe {
    constructor(stripe_pk) {
        this.stripe_pk = stripe_pk;
    }
    async NavigateToPaymentProcessor(isLoggedIn) {
        const c = this.getSelectionCount();
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
        const stripe = await loadStripe(this.stripe_pk);
        stripe.redirectToCheckout({
            customerEmail: "sam.wheat@outlook.com",
            lineItems: [{ price: product, quantity: 1 }],
            successUrl: '/Subscription',
            cancelUrl: '/Subscription',
            mode: 'subscription'
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