import { loadStripe } from '/@stripe/stripe-js';

// ----------------------------------------
// https://github.com/stripe/stripe-js
// https://github.com/stripe/stripe-node#usage-with-typescript
// https://github.com/stripe-samples/accept-a-card-payment/tree/master/using-webhooks/server/node-typescript
// ----------------------------------------

export default class Subsribe {
    private stripe_pk: string;

    constructor(stripe_pk: string) {
        this.stripe_pk = stripe_pk;
    }

    public async NavigateToPaymentProcessor(isLoggedIn: boolean) {
        const c: number = this.getSelectionCount();
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
        $.post('/subscription/accountsetup')

        //const stripe = await loadStripe(this.stripe_pk);
        const stripe = await loadStripe(this.stripe_pk);

        stripe.redirectToCheckout({
            customerEmail: "sam.wheat@outlook.com",
            lineItems: [{ price: product, quantity: 1 }],
            successUrl: '/Subscription',                      //'@Url.Action("Action", "Subscription", null, this.Context.Request.Scheme)'
            cancelUrl: '/Subscription',
            mode: 'subscription'
        } );
    }

    public getSelectionCount() :number {
        return $("#selectionParent > input:checked").length;
    }

    public getSelectedSubscription() {
        return $("#selectionParent > input:checked").attr("data");
    }

    public handleResult(result: any) {
        if (result.error) {
            const displayError = document.getElementById("error-message");
            displayError.textContent = result.error.message;
        }
    }
}