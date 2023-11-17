import { Request } from "./request";
import { InitializationResponse } from "./initialization.response";

export class InitializationRequest extends Request<InitializationResponse> {
    public $type = "Stateflows.Common.InitializationRequest, Stateflows.Common";
}