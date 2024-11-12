export function StateflowsEvent(typeName: string, eventName: string | null = null) {
    return function (target: any) {
        const constructor = (target as Function).prototype.constructor;
        constructor.$type = typeName;
        constructor.eventName = (eventName === null)
            ? typeName.split(',')[0]
            : eventName;
    };
}