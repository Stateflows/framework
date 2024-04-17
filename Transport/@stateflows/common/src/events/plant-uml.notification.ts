import { Notification } from "./notification";

export class PlantUmlNotification extends Notification {
    constructor(
        public plantUml: string,
    ) {
        super();
    }

    public static notificationName = "PlantUmlNotification";
}