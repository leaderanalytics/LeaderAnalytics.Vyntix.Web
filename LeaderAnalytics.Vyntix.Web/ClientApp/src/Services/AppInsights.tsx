import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import AppConfig from '../appconfig';

// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Object_initializer#Computed_property_names

// https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics

export default class AppInsights {

    private static appInsights: ApplicationInsights;

    constructor() {
        // Dont call Init here.  Need to call it manually.
    }

    public static Init(): void {

        this.appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: AppConfig.app_insights_key
            }
        });
        this.appInsights.loadAppInsights();
    }

    public static LogPageView(name?:string, url?:string): void {
        this.appInsights.trackPageView({name: name, uri: url});
    }

    public static LogEvent(name: string, properties?: { [key: string]: any }): void {
        this.appInsights.trackEvent({ name: name }, properties);
    }

    public static LogMetric(name: string, average: number, properties?: { [key: string]: any }): void {
        this.appInsights.trackMetric({ name: name, average: average }, properties);
    }

    public static LogException(exception: Error, severityLevel?: number): void {
        this.appInsights.trackException({ exception: exception, severityLevel: severityLevel });
    }

    public static LogTrace(message: string, properties?: { [key: string]: any }): void {

        this.appInsights.trackTrace({ message: message }, properties);
    }
}
