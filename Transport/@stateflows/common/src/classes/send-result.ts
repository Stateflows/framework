import { EventStatus } from "../enums/event-status";
import { Event } from "../events/event";
import { EventValidation } from "./event-validation";

export class SendResult {
    constructor(
        public Event: Event,
        public Status: EventStatus,
        public Validation: EventValidation,
    ) {}
}