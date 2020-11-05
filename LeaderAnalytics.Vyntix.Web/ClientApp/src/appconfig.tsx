export default class AppConfig {
    //----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
    public static host: string = "https://localhost:5031/";
    public static loginScopes: any = { scopes: ['openid','profile'] };
    public static StripeApiKey: string = "pk_test_51H4XDXLYasEZvdvrSiGSpqqv7qQWsyjomYNsFXjxRWMxWR9lqejkWJmjO3TMlOIi7BmeGzDVz4sooYgHhkZMJCnF00n39Spo0F";
    public static StripeClientId: string = "";
    public static api_url: string = "https://localhost:5031"; 
    public static app_insights_key = "b3548ffb-653a-481e-b900-a1aab30d75df";
}