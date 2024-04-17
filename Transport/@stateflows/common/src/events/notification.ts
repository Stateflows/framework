import { BehaviorId } from "../ids/behavior.id";
import { Event } from "./event";

export class Notification extends Event {
    public static notificationName: string;
    public senderId: BehaviorId;
}