import { StateMachineId } from "../ids/state-machine.id";
import { IStateMachineBehavior } from "./state-machine.behavior";

export interface IStateMachineLocator {
    locateStateMachine(id: StateMachineId): Promise<IStateMachineBehavior>;
}