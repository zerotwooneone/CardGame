import { Observable, Subscription } from 'rxjs';
import { map, shareReplay, filter } from 'rxjs/operators';

export function property<TSource, TResult>(
    // todo: add error handling
    mapper: (s: TSource) => TResult,
    subscriptionCallback?: (subscription: Subscription) => void): (source: Observable<TSource>) => Observable<TResult> {
    return (source: Observable<TSource>) => {
        const result = source.pipe(
            map(mapper),
            filter(value => value !== undefined),
            shareReplay(1)
        );
        const subscription = result.subscribe();
        subscriptionCallback?.(subscription);
        return result;
    };
}


