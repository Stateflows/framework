import { StateflowsEvent } from "../decorators/stateflows-event";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Common.NotificationsResponse, Stateflows.Common")
export class NotificationsResponse extends Event { }