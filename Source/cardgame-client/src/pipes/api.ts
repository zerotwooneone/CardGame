import { Observable, Subscription } from 'rxjs';
import { shareReplay, catchError } from 'rxjs/operators';
import { handleError } from "./handleError";


export function api<TSource>(
    subscriptionCallback?: (subscription: Subscription) => void): (source: Observable<TSource>) => Observable<TSource> {
    return (source: Observable<TSource>) => {
        const result = source.pipe(
            shareReplay(1),
            catchError(handleError)
        );
        const subscription = result.subscribe();
        subscriptionCallback?.(subscription);
        return result;
    };
}
