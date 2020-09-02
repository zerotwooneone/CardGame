
export interface BusSubscription {
    unsubscribe(): void;
    add(subscription: BusSubscription): void;
}
