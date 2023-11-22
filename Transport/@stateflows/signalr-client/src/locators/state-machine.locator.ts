import { Behavior } from "../behaviors/behavior";
import { StateMachine } from "../behaviors/state-machine";
import { StateMachineId } from "../ids/state-machine.id";
import { IBehaviorLocator } from "../interfaces/behavior.locator";
import { IStateMachine } from "../interfaces/state-machine";
import { IStateMachineLocator } from "../interfaces/state-machine.locator";

export class StateMachineLocator implements IStateMachineLocator {
    constructor(private behaviorLocator: IBehaviorLocator) {}

    locateStateMachine(id: StateMachineId): Promise<IStateMachine> {
        return new Promise<IStateMachine>((resolve, reject) => {
            this.behaviorLocator.locateBehavior(id)
                .then(behavior => resolve(new StateMachine(behavior as Behavior)))
                .catch(_ => reject("State Machine not found"));
        });
    }
}