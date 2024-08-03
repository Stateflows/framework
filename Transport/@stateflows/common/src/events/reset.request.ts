import { Request } from "./request";
import { ResetResponse } from "./reset.response";
import { ResetMode } from "../enums/reset-mode";

export class ResetRequest extends Request<ResetResponse> {
    public $type = "Stateflows.Common.ResetRequest, Stateflows.Common";
    public name = "Stateflows.Common.ResetRequest";

    constructor(
        public mode: ResetMode = ResetMode.Full,
    ) {
        super();
    }
}