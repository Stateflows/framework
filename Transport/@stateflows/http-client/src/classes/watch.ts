import { NotificationHandler } from "@stateflows/common";

export class Watch {
    constructor(
        public notificationName: string,
        public milisecondsSinceLastNotificationCheck: number    
    ) {}

    handlers: Array<NotificationHandler<any>> = [];
    notifications: Array<any> = [];
    lastNotificationCheck: string;
}