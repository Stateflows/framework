import { Request } from "./request";
import { CurrentStateResponse } from "./current-state.response";

export class CurrentStateRequest extends Request<CurrentStateResponse> {
    public $type = "Stateflows.StateMachines.Events.CurrentStateRequest, Stateflows.Common";

    constructor() {
        super();
    }
}