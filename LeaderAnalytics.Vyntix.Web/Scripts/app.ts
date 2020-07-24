import Subscribe from './Subscribe.js';

export default class App {

    constructor() {
        alert('this is App');
        const x = 10;
        alert(x);
    }

    public GetSubscriber(stripeKey: string): Subscribe {
        return new Subscribe(stripeKey);
    }

}