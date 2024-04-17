import { Notification } from "../events/notification";

export type NotificationHandler<TNotification extends Notification> = (notification: TNotification) => void;
