import { StateflowsEvent } from "../decorators/stateflows-event";
import { BehaviorInfo } from "./behavior-info";

@StateflowsEvent("Stateflows.StateMachines.Events.StateMachineInfo, Stateflows.Common")
export class StateMachineInfo extends BehaviorInfo {
    public statesStack: Array<string>;
}