import { Event } from "./event";
import { Request } from "./request";
import { CompoundResponse } from "./compound.response";

export class CompoundRequest extends Request<CompoundResponse> {
    public $type = "Stateflows.Common.CompoundRequest, Stateflows.Common";
    public Name = "Stateflows.Common.CompoundRequest";

    constructor(
        public Events: Array<Event>,
    ) {
        super();
    }
}