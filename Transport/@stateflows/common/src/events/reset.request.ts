import { Request } from "./request";
import { ResetResponse } from "./reset.response";

export class ResetRequest extends Request<ResetResponse> {
    public $type = "Stateflows.Common.ResetRequest, Stateflows.Common";
    public name = "Stateflows.Common.ResetRequest";

    constructor(
        public keepVersion: boolean = false,
    ) {
        super();
    }
}