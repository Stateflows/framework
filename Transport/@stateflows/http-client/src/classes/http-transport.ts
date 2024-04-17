import { Event, BehaviorClass, BehaviorId, IStateflowsClientTransport, SendResult, JsonUtils, IWatcher, Notification, CompoundRequest } from "@stateflows/common";
import { NotificationTarget } from "./notification-target";
import { Watch } from "./watch";
import { NotificationsRequest } from "@stateflows/common";

export class HttpTransport implements IStateflowsClientTransport {
    #targets: Map<string, NotificationTarget> = new Map<string, NotificationTarget>();
    #notificationIds: Array<string> = [];

    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }

        setInterval(async () => {
            if (this.#targets.size === 0) {
                return;
            }

            this.#targets.forEach(async target => {
                await this.send(target.behaviorId, new NotificationsRequest());
            });

            // let body = JsonUtils.stringify({
            //     $type: "Stateflows.Common.Transport.Classes.StateflowsNotificationRequest, Stateflows.Common.Transport",
            //     targets: this.getTargets()
            // });

            // let result = await fetch(
            //     `${this.url}stateflows/getNotifications`,
            //     {
            //         method: "POST",
            //         headers: {
            //             'Accept': 'application/json',
            //             'Content-Type': 'application/json'
            //         },
            //         body: body
            //     }
            // );
            
            // let stateflowsNotificationResponse = await result.json();
    
            // this.handleNotifications(stateflowsNotificationResponse.notifications, stateflowsNotificationResponse.responseTime);
        }, 10 * 1000);
    }

    private updateTimestamp(responseTime: string) {
        this.#targets.forEach(target => {
            target.watches.forEach(watch => {
                watch.lastNotificationCheck = responseTime;
                delete watch.milisecondsSinceLastNotificationCheck;
            });
        });
    }

    private handleNotifications(notifications: Array<Notification>, responseTime: string | null = null) {
        if (responseTime !== null) {
            this.updateTimestamp(responseTime);
        }
        
        notifications.forEach((notification: Notification) => {
            if (this.#notificationIds.includes(notification.id)) {
                return;
            }
            delete (notification.senderId.behaviorClass as any).environment;

            let target = this.#targets.get(JsonUtils.stringify(notification.senderId));
            if (typeof target !== 'undefined') {
                target.watches.forEach(watch => {
                    if (watch.notificationName === notification.name) {
                        target.handleNotifications([notification]);
                    }
                });
            }
        });

        this.#notificationIds = notifications.map(notification => notification.id);
    }

    // private getTargets(): Array<any> {
    //     let targets: any[] = [];

    //     this.#targets.forEach((target, behaviorId) => {
    //         let targetIndex = targets.findIndex(t => JsonUtils.stringify(t.id) === JsonUtils.stringify(behaviorId));
    //         if (targetIndex !== -1) {
    //             target.watches.forEach(watch => {
    //                 let watchIndex = targets[targetIndex].watches.findIndex((w: any) => w.notificationName === watch.notificationName);
    //                 if (watchIndex === -1) {
    //                     targets[targetIndex].watches.push({
    //                         notificationName: watch.notificationName,
    //                         lastNotificationCheck: watch.lastNotificationCheck,
    //                         milisecondsSinceLastNotificationCheck: watch.milisecondsSinceLastNotificationCheck !== null
    //                             ? Date.now() - watch.milisecondsSinceLastNotificationCheck
    //                             : null,
    //                     });
    //                 }
    //             });
    //         } else {
    //             targets.push({
    //                 id: target.behaviorId,
    //                 watches: target.watches.map(watch => {
    //                     return {
    //                         notificationName: watch.notificationName,
    //                         lastNotificationCheck: watch.lastNotificationCheck,
    //                         milisecondsSinceLastNotificationCheck: watch.milisecondsSinceLastNotificationCheck !== null
    //                             ? Date.now() - watch.milisecondsSinceLastNotificationCheck
    //                             : null,
    //                     };
    //                 })
    //             });
    //         }
    //     });

    //     return targets;
    // }

    private getWatches(behaviorId: BehaviorId) {
        if (this.#targets.has(JsonUtils.stringify(behaviorId))) {
            let target = this.#targets.get(JsonUtils.stringify(behaviorId));
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
    }

    async getAvailableClasses(): Promise<BehaviorClass[]> {
        let result = await fetch(`${this.url}stateflows/availableClasses`);
        return await result.json() as BehaviorClass[];
    }
    
    async send(behaviorId: BehaviorId, event: Event): Promise<SendResult> {
        const eventNameParts = (event as any).$type.split(',')[0].split('.');
        let eventName = eventNameParts[eventNameParts.length - 1];
        if (eventName === 'CompoundRequest') {
            const eventNames = (event as CompoundRequest).events.map(event => {
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
                    event: event,
                    watches: this.getWatches(behaviorId)
                })
            }
        );
        
        let stateflowsResponse = await result.json();
        let response = stateflowsResponse.response;
        let validation = stateflowsResponse.validation;
        if (response) {
            (event as any).response = response;
        }

        this.handleNotifications(stateflowsResponse.notifications, stateflowsResponse.responseTime);

        let sendResult = new SendResult(event, stateflowsResponse.eventStatus, validation);

        return sendResult;
    }

    async watch(watcher: IWatcher, notificationName: string): Promise<void> {
        let target = this.#targets.has(JsonUtils.stringify(watcher.id))
            ? this.#targets.get(JsonUtils.stringify(watcher.id)) as NotificationTarget
            : new NotificationTarget(watcher);

        this.#targets.set(JsonUtils.stringify(watcher.id), target);

        let watchIndex = target.watches.findIndex(watch => watch.notificationName === notificationName);
        if (watchIndex === -1) {
            target.watches.push(new Watch(notificationName, Date.now()));
        }
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
}