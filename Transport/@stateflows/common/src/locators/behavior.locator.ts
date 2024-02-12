import { Behavior } from "../behaviors/behavior";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IBehavior } from "../interfaces/behavior";
import { IBehaviorLocator } from "../interfaces/behavior.locator";
import { IStateflowsClientTransport } from "../interfaces/stateflows-client-transport";

export class BehaviorLocator implements IBehaviorLocator {
    private behaviorClasses: BehaviorClass[] = [];

    private transportPromise: Promise<IStateflowsClientTransport>;

    constructor(transportPromise: Promise<IStateflowsClientTransport>) {
        this.transportPromise = new Promise<IStateflowsClientTransport>((resolve, reject) => {
            transportPromise
                .then(transport => {
                    transport.getAvailableClasses().then(result => {
                        this.behaviorClasses = result;
                        resolve(transport);
                    });
                })
                .catch(reason => reject(reason));
        });
    }

    locateBehavior(behaviorId: BehaviorId): Promise<IBehavior> {
        return new Promise<IBehavior>((resolve, reject) => {
            this.transportPromise
                .then(hub => {
                    if (this.behaviorClasses.findIndex(behaviorClass => 
                        behaviorClass.type === behaviorId.behaviorClass.type &&
                        behaviorClass.name === behaviorId.behaviorClass.name
                    ) !== -1) {
                        resolve(new Behavior(this.transportPromise, behaviorId));
                    }
                    else
                    {
                        reject("Behavior not found");
                    }
                })
                .catch(reason => reject(reason));
        });
    }
}