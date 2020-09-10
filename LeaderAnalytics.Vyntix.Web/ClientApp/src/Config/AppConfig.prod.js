"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var AppConfig = /** @class */ (function () {
    function AppConfig() {
    }
    //----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
    AppConfig.host = "https://vyntix.com";
    AppConfig.loginScopes = { scopes: ['openid', 'profile'] };
    AppConfig.StripeApiKey = "pk_live_51H4XDXLYasEZvdvrGjxforOOCxAWCrOxLPoJOjeFvFI0DBxCjEEWVrpecKtDtEbxkFTf3ZQSK2AcuGAxP5xPER3c00VFCBO4Sw";
    AppConfig.StripeClientId = "";
    return AppConfig;
}());
exports.default = AppConfig;
//# sourceMappingURL=AppConfig.prod.js.map