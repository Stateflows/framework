import { StateflowsEvent } from "../decorators/stateflows-event";
import { Request } from "./request";
import { StateMachineInfo } from "./state-machine-info";

@StateflowsEvent("Stateflows.StateMachines.StateMachineInfoRequest, Stateflows.Common")
export class StateMachineInfoRequest extends Request<StateMachineInfo> { }