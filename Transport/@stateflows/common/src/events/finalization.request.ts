import { Request } from "./request";
import { FinalizationResponse } from "./finalization.response";

export class FinalizationRequest extends Request<FinalizationResponse> {
    public $type = "Stateflows.Common.FinalizationRequest, Stateflows.Common";
    public name = "Stateflows.Common.FinalizationRequest";
}