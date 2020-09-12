"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var react_1 = require("react");
var react_router_dom_1 = require("react-router-dom");
var react_router_dom_2 = require("react-router-dom");
function ScrollToTop(props) {
    var history = react_router_dom_2.useHistory();
    react_1.useEffect(function () {
        var unlisten = history.listen(function () {
            window.scrollTo(0, 0);
        });
        return function () {
            unlisten();
        };
    }, []);
    return (null);
}
exports.default = react_router_dom_1.withRouter(ScrollToTop);
//# sourceMappingURL=ScrollToTop.js.map