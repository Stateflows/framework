import { Notification, NotificationHandler } from "@stateflows/common";

export class Watch {
    constructor(
        public notificationName: string,
        public milisecondsSinceLastNotificationCheck: number    
    ) {}

    handlers: Array<NotificationHandler<Notification>> = [];
    notifications: Array<Notification> = [];
    lastNotificationCheck: string;
}