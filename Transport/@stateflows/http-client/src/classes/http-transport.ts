import { BehaviorClass, BehaviorId, IStateflowsClientTransport, SendResult, JsonUtils, IWatcher, CompoundRequest } from "@stateflows/common";
import { NotificationTarget } from "./notification-target";
import { Watch } from "./watch";
import { EventHolder, NotificationsRequest } from "@stateflows/common";

export class HttpTransport implements IStateflowsClientTransport {
    #targets: Map<string, NotificationTarget> = new Map<string, NotificationTarget>();
    #notificationIds: Array<string> = [];
    #watchInterval: ReturnType<typeof setInterval>;

    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }

        // setInterval(async () => {
        //     if (this.#targets.size === 0) {
        //         return;
        //     }

        //     this.#targets.forEach(async target => {
        //         await this.send(target.behaviorId, new EventHolder(new NotificationsRequest()));
        //     });
        // }, 10 * 1000);
    }

    private updateTimestamp(responseTime: string) {
        this.#targets.forEach(target => {
            target.watches.forEach(watch => {
                watch.lastNotificationCheck = responseTime;
                delete watch.milisecondsSinceLastNotificationCheck;
            });
        });
    }

    private handleNotifications(notifications: Array<EventHolder>, responseTime: string | null = null) {
        for (const target of this.#targets.values()) {
            target.watches.forEach(watch => {
                watch.lastNotificationCheck = responseTime;
                delete watch.milisecondsSinceLastNotificationCheck;
            });
        }

        const notificationsMap = new Map<string, EventHolder[]>();

        notifications.forEach(notification => {
            if (this.#notificationIds.includes(notification.id)) {
                return;
            }
            delete (notification.senderId as any).$type;
            delete (notification.senderId.behaviorClass as any).$type;
            delete (notification.senderId.behaviorClass as any).environment;
            const senderId = JsonUtils.stringify(notification.senderId);

            notificationsMap.set(
                senderId,
                notificationsMap.has(senderId)
                    ? [...notificationsMap.get(senderId), notification]
                    : [notification]
            );
        });

        for (const senderId of notificationsMap.keys()) {
            const notifications = notificationsMap.get(senderId);
            const target = this.#targets.get(senderId);
            if (typeof target !== 'undefined') {
                const notificationNames = target.watches.map(watch => watch.notificationName);
                target.handleNotifications(notifications.filter(notification => notificationNames.indexOf(notification.name) !== -1));
            }
        }

        this.#notificationIds = notifications.map(notification => notification.id);
        // if (responseTime !== null) {
        //     this.updateTimestamp(responseTime);
        // }
        
        // notifications.forEach(notification => {
        //     if (this.#notificationIds.includes(notification.id)) {
        //         return;
        //     }
        //     delete (notification.senderId.behaviorClass as any).environment;
        //     let target = this.#targets.get(JsonUtils.stringify(notification.senderId));
        //     console.log(notification, this.#targets, JsonUtils.stringify(notification.senderId));

        //     if (typeof target !== 'undefined') {
        //         target.watches.forEach(watch => {
        //             if (watch.notificationName === notification.name) {
        //                 target.handleNotifications([notification.payload]);
        //             }
        //         });
        //     }
        // });

        // this.#notificationIds = notifications.map(notification => notification.id);
    }

    private getWatches(behaviorId: BehaviorId) {
        behaviorId = JsonUtils.deepClone(behaviorId);
        delete (behaviorId as any).$type;
        delete (behaviorId.behaviorClass as any).$type;
        const behaviorIdString = JsonUtils.stringify(behaviorId);
        if (this.#targets.has(behaviorIdString)) {
            let target = this.#targets.get(behaviorIdString);
            return target.watches.map(watch => {
                return {
                    notificationName: watch.notificationName,
                    lastNotificationCheck: watch.lastNotificationCheck,
                    milisecondsSinceLastNotificationCheck: watch.milisecondsSinceLastNotificationCheck !== null
                        ? Date.now() - watch.milisecondsSinceLastNotificationCheck
                        : null,
                };
            });
        } else {
            return [];
        }
        // if (this.#targets.has(JsonUtils.stringify(behaviorId))) {
        //     let target = this.#targets.get(JsonUtils.stringify(behaviorId));
        //     return target.watches.map(watch => {
        //         return {
        //             notificationName: watch.notificationName,
        //             lastNotificationCheck: watch.lastNotificationCheck,
        //             milisecondsSinceLastNotificationCheck: watch.milisecondsSinceLastNotificationCheck !== null
        //                 ? Date.now() - watch.milisecondsSinceLastNotificationCheck
        //                 : null,
        //         };
        //     });
        // } else {
        //     return [];
        // }
    }

    async getAvailableClasses(): Promise<BehaviorClass[]> {
        let result = await fetch(`${this.url}stateflows/availableClasses`);
        return await result.json() as BehaviorClass[];
    }
    
    async send(behaviorId: BehaviorId, event: EventHolder): Promise<SendResult> {
        const eventNameParts = event.payload.$type.split(',')[0].split('.');
        let eventName = eventNameParts[eventNameParts.length - 1];
        if (eventName === 'CompoundRequest') {
            const eventNames = (event.payload as CompoundRequest).events.map(event => {
                const eventNameParts = (event as any).$type.split(',')[0].split('.');
                return eventNameParts[eventNameParts.length - 1];
            });
            eventName = eventNames.join(',');
        }
        let result = await fetch(
            `${this.url}stateflows/send?${eventName}`,
            {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JsonUtils.stringify({
                    "$type": "Stateflows.Common.Transport.Classes.StateflowsRequest, Stateflows.Common.Transport",
                    behaviorId: behaviorId,
                    event,
                    watches: this.getWatches(behaviorId)
                })
            }
        );
        
        let stateflowsResponse = await result.json();
        let response = stateflowsResponse.response;
        let validation = stateflowsResponse.validation;
        if (response) {
            event.payload.response = response.payload;
        }

        this.handleNotifications(stateflowsResponse.notifications, stateflowsResponse.responseTime);

        let sendResult = new SendResult(event, stateflowsResponse.eventStatus, validation);

        return sendResult;
    }

    async watch(watcher: IWatcher, notificationName: string): Promise<void> {
        if (this.#watchInterval == undefined) {
            this.#watchInterval = setInterval(this.watchIntervalCallback.bind(this), 10 * 1000);
        }
        const behaviorId = JsonUtils.deepClone(watcher.id);
        delete (behaviorId as any).$type;
        delete (behaviorId.behaviorClass as any).$type;
        const behaviorIdString = JsonUtils.stringify(behaviorId);
        let target = this.#targets.has(behaviorIdString)
            ? this.#targets.get(behaviorIdString) as NotificationTarget
            : new NotificationTarget(watcher);

        this.#targets.set(behaviorIdString, target);

        let watchIndex = target.watches.findIndex(watch => watch.notificationName === notificationName);
        if (watchIndex === -1) {
            target.watches.push(new Watch(notificationName, Date.now()));
        }
        // let target = this.#targets.has(JsonUtils.stringify(watcher.id))
        //     ? this.#targets.get(JsonUtils.stringify(watcher.id)) as NotificationTarget
        //     : new NotificationTarget(watcher);

        // this.#targets.set(JsonUtils.stringify(watcher.id), target);

        // let watchIndex = target.watches.findIndex(watch => watch.notificationName === notificationName);
        // if (watchIndex === -1) {
        //     target.watches.push(new Watch(notificationName, Date.now()));
        // }
    }

    async unwatch(watcher: IWatcher, notificationName: string): Promise<void> {
        if (this.#targets.has(JsonUtils.stringify(watcher.id))) {
            let target = this.#targets.get(JsonUtils.stringify(watcher.id)) as NotificationTarget;
            let index = target.watches.findIndex(watch => watch.notificationName === notificationName);
            if (index !== -1) {
                delete target.watches[index];
            }
            this.#targets.delete(JsonUtils.stringify(watcher.id));
        }
    }

    private async watchIntervalCallback() {
        if (this.#targets.size === 0) {
            return;
        }

        this.#targets.forEach(async target => {
            await this.send(target.behaviorId, new EventHolder(new NotificationsRequest()));
        });
    } 
}