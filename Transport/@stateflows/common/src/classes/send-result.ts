import { EventStatus } from "../enums/event-status";
import { Event } from "../events/event";
import { EventValidation } from "./event-validation";

export class SendResult {
    constructor(
        public event: Event,
        public status: EventStatus,
        public validation: EventValidation,
    ) {}
}