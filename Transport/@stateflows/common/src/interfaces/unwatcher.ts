export interface IUnwatcher {
    unwatch(notificationName: string): Promise<void>;
}