export class ValidationResult {
    constructor(
        public ErrorMessage: string | null,
        public MemberNames: Array<string> | null,
    ) {}
}