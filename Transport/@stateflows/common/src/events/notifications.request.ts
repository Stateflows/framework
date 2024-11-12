import { Request } from "./request";
import { NotificationsResponse } from "./notifications.response";
import { StateflowsEvent } from "../decorators/stateflows-event";

@StateflowsEvent("Stateflows.Common.NotificationsRequest, Stateflows.Common")
export class NotificationsRequest extends Request<NotificationsResponse> { }