import { IWatcher } from "../interfaces/watcher";
import { IUnwatcher } from "../interfaces/unwatcher";

export class Watcher implements IWatcher, Disposable {
    constructor(
        private notificationName: string,
        private unwatcher: IUnwatcher
    ) {}

    [Symbol.dispose](): void {
        this.unwatcher.unwatch(this.notificationName);
    }

    async unwatch(): Promise<void> {
        this[Symbol.dispose]();
    }
}
