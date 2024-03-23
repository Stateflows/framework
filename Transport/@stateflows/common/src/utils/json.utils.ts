export class JsonUtils {
    public static stringify(object: any): string {
        const replacer = (key: string, value: any) =>
            value instanceof Object && !(value instanceof Array)
                ? Object.keys(value)
                    .sort()
                    .reduce(
                        (sorted, key) => {
                            sorted[key] = value[key];
                            return sorted;
                        },
                        {}
                    )
                : value;

        return JSON.stringify(object, replacer);
    }

    public static parse(json: string): any {
        return JSON.parse(json);
    }
}