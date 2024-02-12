import { BehaviorStatus } from "../enums/behavior-status";
import { Response } from "./response";

export class CurrentStateResponse extends Response {
    constructor(
        public StatesStack: string[],
        public ExpectedEvents: string[],  
    ) {
        super();
    }
}