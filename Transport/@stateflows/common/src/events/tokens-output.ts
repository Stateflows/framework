import { StateflowsEvent } from "../decorators/stateflows-event";
import { TokensTransfer } from "./tokens-transfer";

@StateflowsEvent("Stateflows.Activities.Events.TokensOutput, Stateflows.Common")
export class TokensOutput extends TokensTransfer { }