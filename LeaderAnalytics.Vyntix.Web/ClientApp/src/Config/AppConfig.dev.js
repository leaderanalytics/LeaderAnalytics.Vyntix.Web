"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var AppConfig = /** @class */ (function () {
    function AppConfig() {
    }
    //----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
    AppConfig.host = "https://localhost:5031/";
    AppConfig.loginScopes = { scopes: ['openid', 'profile'] };
    AppConfig.StripeApiKey = "pk_test_51H4XDXLYasEZvdvrSiGSpqqv7qQWsyjomYNsFXjxRWMxWR9lqejkWJmjO3TMlOIi7BmeGzDVz4sooYgHhkZMJCnF00n39Spo0F";
    AppConfig.StripeClientId = "";
    return AppConfig;
}());
exports.default = AppConfig;
//# sourceMappingURL=AppConfig.dev.js.map