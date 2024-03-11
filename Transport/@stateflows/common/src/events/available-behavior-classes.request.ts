import { Request } from "./request";
import { AvailableBehaviorClassesResponse } from "./available-behavior-classes.response";

export class AvailableBehaviorClassesRequest extends Request<AvailableBehaviorClassesResponse> {
    public $type = "Stateflows.System.AvailableBehaviorClassesRequest, Stateflows.Common";
    public name = "Stateflows.System.AvailableBehaviorClassesRequest";

    constructor() {
        super();
    }
}