import { BehaviorClass } from "./behavior.class";
import { BehaviorId } from "./behavior.id";

export class StateMachineId extends BehaviorId {
    constructor(
        name: string,
        instance: string
    ) {
        super(new BehaviorClass("StateMachine", name), instance);
    }
}