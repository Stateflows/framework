import { StateflowsEvent } from "../decorators/stateflows-event";
import { BehaviorInfo } from "./behavior-info";

export interface TreeNode<T> {
    value: T;
    nodes: Array<TreeNode<T>>;
}

export interface Tree<T> {
    value: T;
    root: TreeNode<T>;
}

@StateflowsEvent("Stateflows.StateMachines.StateMachineInfo, Stateflows.Common")
export class StateMachineInfo extends BehaviorInfo {
    public statesTree: Tree<string>;
}