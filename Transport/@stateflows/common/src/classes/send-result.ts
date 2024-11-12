import { EventStatus } from "../enums/event-status";
import { EventValidation } from "./event-validation";

export class SendResult {
    constructor(
        public event: any,
        public status: EventStatus,
        public validation: EventValidation,
    ) {}
}