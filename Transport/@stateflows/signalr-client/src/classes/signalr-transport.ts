import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { Event, BehaviorClass, BehaviorId, IStateflowsClientTransport, SendResult } from "@stateflows/common";

export class SignalRTransport implements IStateflowsClientTransport {
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

    private get reconnectingHub(): Promise<HubConnection> {
        return new Promise<HubConnection>(async (resolve, reject) => {
            let hub = await this.hub;
            if (hub.state != HubConnectionState.Connected) {
                hub.start()
                    .then(() => resolve(hub))
                    .catch(reason => reject(reason));
            } else {
                resolve(hub);
            }
        });
    }

    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }
    }

    getAvailableClasses(): Promise<BehaviorClass[]> {
        return new Promise<BehaviorClass[]>(async (resolve, reject) => {
            let hub = await this.reconnectingHub;
            let result = await hub.invoke('GetAvailableClasses');
            resolve(result);
        });
    }
    
    send(behaviorId: BehaviorId, event: Event): Promise<SendResult> {
        return new Promise<SendResult>(async (resolve, reject) => {
            let hub = await this.reconnectingHub;
            let resultString = await hub.invoke("Send", behaviorId, JSON.stringify(event));
            let result = JSON.parse(resultString);
            if (result.Response) {
                (event as any).Response = result.Response;
                delete result.Response;
            }
            resolve(new SendResult(event, result.EventStatus, result.Validation));
        });
    }
}