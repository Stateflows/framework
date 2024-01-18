import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { IBehaviorLocator } from "../interfaces/behavior.locator";
import { IStateMachineLocator } from "../interfaces/state-machine.locator";
import { BehaviorLocator } from "../locators/behavior.locator";
import { StateMachineLocator } from "../locators/state-machine.locator";
import { ISystem } from "../interfaces/system";
import { BehaviorId } from "../ids/behavior.id";
import { BehaviorClass } from "../ids/behavior.class";
import { System } from "../behaviors/system";
import { IActivityLocator } from "../interfaces/activity.locator";
import { ActivityLocator } from "../locators/activity.locator";

export class StateflowsClient {
    #hub: Promise<HubConnection> | null = null;

    private get hub(): Promise<HubConnection> {
        if (this.#hub == null) {
            this.#hub = new Promise<HubConnection>((resolve, reject) => {
                let hub = new HubConnectionBuilder()
                    .withUrl(this.url + "stateflows_v1")
                    .build();

                hub.start().then(() => resolve(hub));
            });
        }

        return this.#hub;
    }

    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }
    }

    #behaviorLocator: IBehaviorLocator | null = null;

    public get behaviorLocator(): IBehaviorLocator {
        return this.#behaviorLocator ??= new BehaviorLocator(this.hub);
    }

    #stateMachineLocator: IStateMachineLocator | null = null;

    public get stateMachineLocator(): IStateMachineLocator {
        return this.#stateMachineLocator ??= new StateMachineLocator(this.behaviorLocator);
    }

    #activityLocator: IActivityLocator | null = null;

    public get activityLocator(): IActivityLocator {
        return this.#activityLocator ??= new ActivityLocator(this.behaviorLocator);
    }

    #systemPromise: Promise<ISystem> | null = null;

    public get system(): Promise<ISystem> {
        return this.#systemPromise ??= new Promise<ISystem>((resolve, reject) => {
            this.behaviorLocator.locateBehavior(new BehaviorId(new BehaviorClass("System", "Stateflows"), ""))
                .then(behavior => resolve(new System(behavior)))
                .catch(reason => reject(reason));
        })
    }
}