export default class AppConfig {
    //----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
    public static host: string = "https://vyntix.com";
    public static loginScopes: any = { scopes: ['openid', 'profile'] };
    public static StripeApiKey : string = "pk_live_51H4XDXLYasEZvdvrGjxforOOCxAWCrOxLPoJOjeFvFI0DBxCjEEWVrpecKtDtEbxkFTf3ZQSK2AcuGAxP5xPER3c00VFCBO4Sw";
    public static StripeClientId: string = "";
    public static api_url: string = 'https://leaderanalytics.com/api';
}