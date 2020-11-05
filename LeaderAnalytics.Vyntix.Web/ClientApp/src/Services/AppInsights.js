"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var applicationinsights_web_1 = require("@microsoft/applicationinsights-web");
var appconfig_1 = require("../appconfig");
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Object_initializer#Computed_property_names
// https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics
var AppInsights = /** @class */ (function () {
    function AppInsights() {
        // Dont call Init here.  Need to call it manually.
    }
    AppInsights.Init = function () {
        this.appInsights = new applicationinsights_web_1.ApplicationInsights({
            config: {
                instrumentationKey: appconfig_1.default.app_insights_key
            }
        });
        this.appInsights.loadAppInsights();
    };
    AppInsights.LogPageView = function (name, url) {
        this.appInsights.trackPageView({ name: name, uri: url });
    };
    AppInsights.LogEvent = function (name, properties) {
        this.appInsights.trackEvent({ name: name }, properties);
    };
    AppInsights.LogMetric = function (name, average, properties) {
        this.appInsights.trackMetric({ name: name, average: average }, properties);
    };
    AppInsights.LogException = function (exception, severityLevel) {
        this.appInsights.trackException({ exception: exception, severityLevel: severityLevel });
    };
    AppInsights.LogTrace = function (message, properties) {
        this.appInsights.trackTrace({ message: message }, properties);
    };
    return AppInsights;
}());
exports.default = AppInsights;
//# sourceMappingURL=AppInsights.js.map