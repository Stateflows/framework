import { Response } from "./response";

export class ResetResponse extends Response {
    constructor(
        public resetSuccessful: boolean,
    ) {
        super();
    }
}