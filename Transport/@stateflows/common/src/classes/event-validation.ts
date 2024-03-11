import { ValidationResult } from "./validation-result";

export class EventValidation {
    constructor(
        public isValid: boolean,
        public validationResults: Array<ValidationResult>,
    ) {}
}