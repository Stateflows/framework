export class ValidationResult {
    constructor(
        public errorMessage: string | null,
        public memberNames: Array<string> | null,
    ) {}
}