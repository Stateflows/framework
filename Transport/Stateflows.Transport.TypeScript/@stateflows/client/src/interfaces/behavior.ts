import { Request } from "../events/request";

export interface IBehavior {
    send(event: Event): Promise<boolean>;
    request<TResponse>(request: Request<TResponse>): Promise<TResponse>;

    initialize(): Promise<boolean>;
    isInitialized(): Promise<boolean>;
}