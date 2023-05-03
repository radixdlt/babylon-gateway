import { StreamApi } from "../generated";

export class Stream {
    constructor(public innerClient: StreamApi) {
    }
}