import { EventStatus } from "../enums/event-status";
import { EventValidation } from "./event-validation";
import { SendResult } from "./send-result";

export class RequestResult<TResponse> extends SendResult {
    constructor(
        public response: TResponse,
        event: any,
        status: EventStatus,
        validation: EventValidation,
    ) {
        super(event, status, validation);
    }
}