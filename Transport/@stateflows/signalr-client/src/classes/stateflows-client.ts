import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { IBehaviorLocator } from "../interfaces/behavior.locator";
import { IStateMachineLocator } from "../interfaces/state-machine.locator";
import { BehaviorLocator } from "../locators/behavior.locator";
import { StateMachineLocator } from "../locators/state-machine.locator";
import { ISystem } from "../interfaces/system";
import { BehaviorId } from "../ids/behavior.id";
import { BehaviorClass } from "../ids/behavior.class";
import { System } from "../behaviors/system";
import { Behavior } from "../behaviors/behavior";

export class StateflowsClient {
    private _hub: Promise<HubConnection> | null = null;

    private get hub(): Promise<HubConnection> {
        if (this._hub == null) {
            this._hub = new Promise<HubConnection>((resolve, reject) => {
                let hub = new HubConnectionBuilder()
                    .withUrl(this.url + "stateflows_v1")
                    .build();

                hub.start().then(() => resolve(hub));
            });
        }

        return this._hub;
    }

    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }
    }

    private _behaviorLocator: IBehaviorLocator | null = null;

    public get behaviorLocator(): IBehaviorLocator {
        return this._behaviorLocator ??= new BehaviorLocator(this.hub);
    }

    private _stateMachineLocator: IStateMachineLocator | null = null;

    public get stateMachineLocator(): IStateMachineLocator {
        return this._stateMachineLocator ??= new StateMachineLocator(this.behaviorLocator);
    }

    public _systemPromise: Promise<ISystem> | null = null;

    public get system(): Promise<ISystem> {
        return this._systemPromise ??= new Promise<ISystem>((resolve, reject) => {
            this.behaviorLocator.locateBehavior(new BehaviorId(new BehaviorClass("System", "Stateflows"), ""))
                .then(behavior => resolve(new System(behavior)))
                .catch(reason => reject(reason));
        })
    }
}