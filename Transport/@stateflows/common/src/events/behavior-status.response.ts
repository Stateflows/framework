import { Response } from "./response";
import { BehaviorStatus } from "../enums/behavior-status";

export class BehaviorStatusResponse extends Response {
    constructor(
        public BehaviorStatus: BehaviorStatus,
    ) {
        super();
    }
}