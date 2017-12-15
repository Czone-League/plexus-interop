
import { InteropClient } from "./InteropClient";
import { Injectable } from "@angular/core";

@Injectable()
export class IntropClientFactory {

    public connect(appId: string): Promise<InteropClient> {
        return Promise.reject("Not implemented");
    }

}