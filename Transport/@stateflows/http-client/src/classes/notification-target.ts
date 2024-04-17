import { BehaviorId } from "@stateflows/common";
import { Notification } from "@stateflows/common";
import { IWatcher } from "@stateflows/common";
import { Watch } from "./watch";

export class NotificationTarget {
    #watcher: IWatcher;

    constructor(watcher: IWatcher) {
        this.#watcher = watcher;
    }

    watches: Array<Watch> = [];

    get behaviorId(): BehaviorId {
        return this.#watcher.id;
    }

    handleNotifications(notifications: Array<Notification>) {
        let notificationNames = this.watches.map(watch => watch.notificationName);
        notifications.forEach(notification => {
            if (notificationNames.indexOf(notification.name) !== -1) {
                this.#watcher.notify(notification);
            }
        });
    }
}