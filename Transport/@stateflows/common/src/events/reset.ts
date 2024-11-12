import { StateflowsEvent } from "../decorators/stateflows-event";
import { ResetMode } from "../enums/reset-mode";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Common.Reset, Stateflows.Common")
export class Reset extends Event {
    constructor(
        public mode: ResetMode = ResetMode.Full,
    ) {
        super();
    }
}