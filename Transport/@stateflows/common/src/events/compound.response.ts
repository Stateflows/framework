import { Response } from "./response";
import { RequestResult } from "../classes/request-result";

export class CompoundResponse extends Response {
    constructor(
        public results: Array<RequestResult<Response>>,
    ) {
        super();
    }
}
