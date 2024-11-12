import { StateflowsEvent } from "../decorators/stateflows-event";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Common.Initialize, Stateflows.Common")
export class Initialize extends Event { }