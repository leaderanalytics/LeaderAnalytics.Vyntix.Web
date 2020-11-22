export default class UserRecord {
    public ID: string = "";
    public BillingID: string = "";
    public IsBanned: boolean = false;
    public IsOptIn: string = "";
    public IsCorporateAdmin: string = "";
    public PaymentProviderCustomerID: string = "";
    public SuspendedUntil: Date | null = null;

}