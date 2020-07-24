import Subscribe from './Subscribe.js';
export default class App {
    constructor() {
        alert('this is App');
        const x = 10;
        alert(x);
    }
    GetSubscriber(stripeKey) {
        return new Subscribe(stripeKey);
    }
}
//# sourceMappingURL=app.js.map