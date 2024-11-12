import { Event } from "./event";

export abstract class Request<TResponse> extends Event {
    public response: TResponse;
}