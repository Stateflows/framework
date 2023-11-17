import { IBehavior } from "./behavior";

export interface IStateMachine extends IBehavior {
    getCurrentState(): Promise<any>;
    getExpectedEvents(): Promise<string[]>;
}