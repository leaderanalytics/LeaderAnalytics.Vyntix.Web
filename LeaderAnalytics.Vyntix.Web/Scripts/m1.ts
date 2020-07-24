import m2 from './m2.js';

export default class m1 {

    constructor() {
        alert('this is m1');
        const m = new m2();
    }

    getm2() {
        return new m2();
    }
}