import { StateMachineId } from "../ids/state-machine.id";
import { IStateMachine } from "./state-machine";

export interface IStateMachineLocator {
    locateStateMachine(id: StateMachineId): Promise<IStateMachine>;
}