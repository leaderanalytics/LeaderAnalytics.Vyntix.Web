import SubscriptionPlan from './SubscriptionPlan';

export default class CorpSubscriptionInfo {
    public AdminEmail: string = "";
    public SubscriptionPlan: SubscriptionPlan | null = new SubscriptionPlan();         
}