import { StateflowsEvent } from "../decorators/stateflows-event";
import { Request } from "./request";
import { TokensOutput } from "./tokens-output";

@StateflowsEvent("Stateflows.Activities.TokensOutputRequest, Stateflows.Common")
export class TokensOutputRequest extends Request<TokensOutput> {
    constructor(private tokenName: string | undefined) {
        super();
        
    }

    public get $type(): string {
        return (this as unknown as any).constructor.$type + (typeof this.tokenName === "undefined" ? "" : "");
    }
    
    public get eventName(): string {
        return (this as unknown as any).constructor.eventName;
    }
}