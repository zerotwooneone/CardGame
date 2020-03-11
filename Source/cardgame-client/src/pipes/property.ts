import { Observable, Subscription } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

export function property<TSource, TResult>(
    mapper: (s: TSource) => TResult,
    subscriptionCallback?: (subscription: Subscription) => void): (source: Observable<TSource>) => Observable<TResult> {
    return (source: Observable<TSource>) => {
        const result = source.pipe(
            map(mapper),
            shareReplay(1)
        );
        const subscription = result.subscribe();
        subscriptionCallback?.(subscription);
        return result;
    };
}
