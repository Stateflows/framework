import { Response } from "./response";
import { Event } from "./event";

export class Request<TResponse extends Response> extends Event {
    public response: TResponse;
}