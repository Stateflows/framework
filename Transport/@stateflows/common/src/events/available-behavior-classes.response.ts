import { Response } from "./response";
import { BehaviorClass } from "../ids/behavior.class";

export class AvailableBehaviorClassesResponse extends Response {
    constructor(
        public availableBehaviorClasses: Array<BehaviorClass>,
    ) {
        super();
    }
}