import { CurrentStateRequest } from "../events/current-state.request";
import { IStateMachine } from "../interfaces/state-machine";
import { Behavior } from "./behavior";

export class StateMachine extends Behavior implements IStateMachine {
    constructor(behavior: Behavior) {
        super(behavior, behavior.behaviorId);
    }

    getCurrentState(): Promise<any> {
        return this.request(new CurrentStateRequest());
    }
}