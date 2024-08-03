import { IBehaviorLocator } from "../interfaces/behavior.locator";
import { IStateMachineLocator } from "../interfaces/state-machine.locator";
import { BehaviorLocator } from "../locators/behavior.locator";
import { StateMachineLocator } from "../locators/state-machine.locator";
import { BehaviorId } from "../ids/behavior.id";
import { BehaviorClass } from "../ids/behavior.class";
import { IActivityLocator } from "../interfaces/activity.locator";
import { ActivityLocator } from "../locators/activity.locator";
import { IStateflowsClientTransportFactory } from "../interfaces/stateflows-client-transport-factory";

export class StateflowsClient {
    constructor(private transportFactory: IStateflowsClientTransportFactory) { }

    #behaviorLocator: IBehaviorLocator | null = null;

    public get behaviorLocator(): IBehaviorLocator {
        return this.#behaviorLocator ??= new BehaviorLocator(this.transportFactory.getTransport());
    }

    #stateMachineLocator: IStateMachineLocator | null = null;

    public get stateMachineLocator(): IStateMachineLocator {
        return this.#stateMachineLocator ??= new StateMachineLocator(this.behaviorLocator);
    }

    #activityLocator: IActivityLocator | null = null;

    public get activityLocator(): IActivityLocator {
        return this.#activityLocator ??= new ActivityLocator(this.behaviorLocator);
    }
}