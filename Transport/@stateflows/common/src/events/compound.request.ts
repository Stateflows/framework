import { Request } from "./request";
import { CompoundResponse } from "./compound.response";
import { EventHolder } from "../classes/event-holder";
import { StateflowsEvent } from "../decorators/stateflows-event";

@StateflowsEvent("Stateflows.Common.CompoundRequest, Stateflows.Common")
export class CompoundRequest extends Request<CompoundResponse> {
    constructor(
        events: Array<any>,
    ) {
        super();
        this.events = events.map(event => new EventHolder(event));
    }

    public events: Array<EventHolder>;
}
