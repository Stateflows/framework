import { StateflowsEvent } from "../decorators/stateflows-event";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Common.Finalize, Stateflows.Common")
export class Finalize extends Event {}