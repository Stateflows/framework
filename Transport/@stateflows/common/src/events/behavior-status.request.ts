import { Request } from "./request";
import { BehaviorStatusResponse } from "./behavior-status.response";

export class BehaviorStatusRequest extends Request<BehaviorStatusResponse> {
    public $type = "Stateflows.Common.BehaviorStatusRequest, Stateflows.Common";

    constructor() {
        super();
    }
}