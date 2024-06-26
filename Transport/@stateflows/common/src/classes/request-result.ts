import { EventStatus } from "../enums/event-status";
import { Event } from "../events/event";
import { EventValidation } from "./event-validation";
import { SendResult } from "./send-result";

export class RequestResult<TResponse> extends SendResult {
    constructor(
        public response: TResponse,
        event: Event,
        status: EventStatus,
        validation: EventValidation,
    ) {
        super(event, status, validation);
    }
}