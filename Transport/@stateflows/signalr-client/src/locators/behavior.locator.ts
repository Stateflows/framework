import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { Behavior } from "../behaviors/behavior";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IBehavior } from "../interfaces/behavior";
import { IBehaviorLocator } from "../interfaces/behavior.locator";

export class BehaviorLocator implements IBehaviorLocator {
    private behaviorClasses: BehaviorClass[] = [];

    private hubPromise: Promise<HubConnection>;

    constructor(hubPromise: Promise<HubConnection>) {
        this.hubPromise = new Promise<HubConnection>((resolve, reject) => {
            hubPromise
                .then(hub => {
                    hub.invoke<BehaviorClass[]>('GetAvailableClasses').then(result => {
                        this.behaviorClasses = result;
                        console.log(result);
                        resolve(hub);
                    });
                })
                .catch(reason => reject(reason));
        });
    }

    private getHubReconnectionPromise(hub: HubConnection): Promise<HubConnection> {
        return new Promise<HubConnection>(async (resolve, reject) => {
            if (hub.state != HubConnectionState.Connected) {
                hub.start()
                    .then(() => resolve(hub))
                    .catch(reason => reject(reason));
            } else {
                resolve(hub);
            }
        });
    }

    locateBehavior(behaviorId: BehaviorId): Promise<IBehavior> {
        return new Promise<IBehavior>((resolve, reject) => {
            this.hubPromise
                .then(hub => {
                    if (this.behaviorClasses.findIndex(behaviorClass => 
                        behaviorClass.type === behaviorId.behaviorClass.type &&
                        behaviorClass.name === behaviorId.behaviorClass.name
                    ) !== -1) {
                        resolve(new Behavior(this.getHubReconnectionPromise(hub), behaviorId));
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