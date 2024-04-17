import { Request } from "./request";
import { NotificationsResponse } from "./notifications.response";

export class NotificationsRequest extends Request<NotificationsResponse> {
    public $type = "Stateflows.Common.NotificationsRequest, Stateflows.Common";
    public name = "Stateflows.Common.NotificationsRequest";

    constructor() {
        super();
    }
}