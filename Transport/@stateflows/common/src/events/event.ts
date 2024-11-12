export abstract class Event {
    public static $type: string;

    public static eventName: string;

    public get $type(): string {
        return (this as unknown as any).constructor.$type;
    }
    
    public get eventName(): string {
        return (this as unknown as any).constructor.eventName;
    }
}