import { ValidationResult } from "./validation-result";

export class EventValidation {
    constructor(
        public IsValid: boolean,
        public ValidationResults: Array<ValidationResult>,
    ) {}
}