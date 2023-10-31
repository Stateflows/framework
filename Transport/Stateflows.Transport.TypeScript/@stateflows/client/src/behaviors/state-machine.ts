import { IStateMachine } from "../interfaces/state-machine";
import { Behavior } from "./behavior";

export class StateMachine extends Behavior implements IStateMachine {
    constructor(behavior: Behavior) {
        super(behavior);
    }

    getCurrentState(): Promise<any> {
        throw new Error("Method not implemented.");
    }
    
    getExpectedEvents(): Promise<string[]> {
        throw new Error("Method not implemented.");
    }
}