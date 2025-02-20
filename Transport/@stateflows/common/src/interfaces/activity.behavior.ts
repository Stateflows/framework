import { IWatcher } from "./watcher";
import { IBehavior } from "./behavior";
import { SendResult } from "../classes/send-result";
import { RequestResult } from "../classes/request-result";
import { NotificationHandler } from "../utils/notification-handler";

export interface IActivityBehavior extends IBehavior {
    // getOutputAsync(): Promise<RequestResult<TokensOutput>>;
    // getOutputAsync(notificationName: string): Promise<RequestResult<TokensOutput>>;
    // // sendInputAsync(Action<ITokensInput> tokensAction): Promise<SendResult>;
    // sendInputAsync<TToken>(notificationName: string, tokens: Array<TToken>): Promise<SendResult>;
    // watchOutputAsync(handler: NotificationHandler<TokensOutput>): Promise<IWatcher>;
    // watchOutputAsync<TToken>(notificationName: string, handler: NotificationHandler<TToken>): Promise<IWatcher>;
}