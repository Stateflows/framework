import { Request } from "./request";
import { BehaviorInstancesResponse } from "./behavior-instances.response";

export class BehaviorInstancesRequest extends Request<BehaviorInstancesResponse> {
    public $type = "Stateflows.System.BehaviorInstancesRequest, Stateflows.Common";
    public Name = "Stateflows.System.BehaviorInstancesRequest";

    constructor() {
        super();
    }
}