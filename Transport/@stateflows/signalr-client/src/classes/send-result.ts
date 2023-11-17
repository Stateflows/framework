import { EventStatus } from "../enums/event-status";
import { EventValidation } from "./event-validation";

export class SendResult {
    constructor(
        public Event: Event,
        public Status: EventStatus,
        public Validation: EventValidation,
    ) {}
}