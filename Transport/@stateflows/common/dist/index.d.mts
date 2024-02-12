declare class BehaviorClass {
    type: string;
    name: string;
    constructor(type: string, name: string);
    $type: string;
}

declare class BehaviorId {
    behaviorClass: BehaviorClass;
    instance: string;
    constructor(behaviorClass: BehaviorClass, instance: string);
    $type: string;
}

declare enum EventStatus {
    Undelivered = 0,
    Rejected = 1,
    Invalid = 2,
    NotConsumed = 3,
    Deferred = 4,
    Consumed = 5
}

declare class Event {
}

declare class ValidationResult {
    ErrorMessage: string | null;
    MemberNames: Array<string> | null;
    constructor(ErrorMessage: string | null, MemberNames: Array<string> | null);
}

declare class EventValidation {
    IsValid: boolean;
    ValidationResults: Array<ValidationResult>;
    constructor(IsValid: boolean, ValidationResults: Array<ValidationResult>);
}

declare class SendResult {
    Event: Event;
    Status: EventStatus;
    Validation: EventValidation;
    constructor(Event: Event, Status: EventStatus, Validation: EventValidation);
}

declare class RequestResult<TResponse> extends SendResult {
    Response: TResponse;
    constructor(Response: TResponse, event: Event, status: EventStatus, validation: EventValidation);
}

declare enum BehaviorStatus {
    Unknown = 0,
    NotInitialized = 1,
    Initialized = 2,
    Finalized = 3
}

declare class Response extends Event {
}

declare class BehaviorStatusResponse extends Response {
    BehaviorStatus: BehaviorStatus;
    constructor(BehaviorStatus: BehaviorStatus);
}

declare class InitializationResponse extends Response {
    InitializationSuccessful: boolean;
    constructor(InitializationSuccessful: boolean);
}

declare class Request<TResponse extends Response> extends Event {
    Response: TResponse;
}

declare class InitializationRequest extends Request<InitializationResponse> {
    $type: string;
}

interface IBehavior {
    send(event: Event): Promise<SendResult>;
    request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>>;
    initialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>>;
    getStatus(): Promise<RequestResult<BehaviorStatusResponse>>;
}

interface IBehaviorLocator {
    locateBehavior(id: BehaviorId): Promise<IBehavior>;
}

declare class StateMachineId extends BehaviorId {
    constructor(name: string, instance: string);
}

declare class CurrentStateResponse extends Response {
    StatesStack: string[];
    ExpectedEvents: string[];
    constructor(StatesStack: string[], ExpectedEvents: string[]);
}

interface IStateMachine extends IBehavior {
    getCurrentState(): Promise<RequestResult<CurrentStateResponse>>;
}

interface IStateMachineLocator {
    locateStateMachine(id: StateMachineId): Promise<IStateMachine>;
}

declare class AvailableBehaviorClassesResponse extends Response {
    AvailableBehaviorClasses: BehaviorClass[];
    constructor(AvailableBehaviorClasses: BehaviorClass[]);
}

declare class BehaviorDescriptor {
    Id: BehaviorId;
    Status: BehaviorStatus;
}
declare class BehaviorInstancesResponse extends Response {
    Behaviors: BehaviorDescriptor[];
    constructor(Behaviors: BehaviorDescriptor[]);
}

interface ISystem {
    getAvailableBehaviorClasses(): Promise<RequestResult<AvailableBehaviorClassesResponse>>;
    getBehaviorInstances(): Promise<RequestResult<BehaviorInstancesResponse>>;
}

declare class ActivityId extends BehaviorId {
    constructor(name: string, instance: string);
}

interface IActivity extends IBehavior {
}

interface IActivityLocator {
    locateActivity(id: ActivityId): Promise<IActivity>;
}

interface IStateflowsClientTransport {
    getAvailableClasses(): Promise<BehaviorClass[]>;
    send(behaviorId: BehaviorId, event: Event): Promise<SendResult>;
}

interface IStateflowsClientTransportFactory {
    getTransport(): Promise<IStateflowsClientTransport>;
}

declare class StateflowsClient {
    #private;
    private transportFactory;
    constructor(transportFactory: IStateflowsClientTransportFactory);
    get behaviorLocator(): IBehaviorLocator;
    get stateMachineLocator(): IStateMachineLocator;
    get activityLocator(): IActivityLocator;
    get system(): Promise<ISystem>;
}

declare class BehaviorStatusRequest extends Request<BehaviorStatusResponse> {
    $type: string;
    constructor();
}

declare class PlantUmlResponse extends Response {
    PlantUml: string;
    constructor(PlantUml: string);
}

declare class PlantUmlRequest extends Request<PlantUmlResponse> {
    $type: string;
    constructor();
}

declare class AvailableBehaviorClassesRequest extends Request<AvailableBehaviorClassesResponse> {
    $type: string;
    constructor();
}

declare class BehaviorInstancesRequest extends Request<BehaviorInstancesResponse> {
    $type: string;
    constructor();
}

export { ActivityId, AvailableBehaviorClassesRequest, AvailableBehaviorClassesResponse, BehaviorClass, BehaviorId, BehaviorInstancesRequest, BehaviorInstancesResponse, BehaviorStatus, BehaviorStatusRequest, BehaviorStatusResponse, Event, EventStatus, type IActivity, type IActivityLocator, type IBehavior, type IBehaviorLocator, type IStateMachine, type IStateMachineLocator, type IStateflowsClientTransport, type IStateflowsClientTransportFactory, type ISystem, InitializationRequest, InitializationResponse, PlantUmlRequest, PlantUmlResponse, Request, RequestResult, Response, SendResult, StateMachineId, StateflowsClient };
