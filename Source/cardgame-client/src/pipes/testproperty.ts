import { Observable } from 'rxjs';
import { timeout, take } from 'rxjs/operators';

export function testproperty<TSource>(source: Observable<TSource>) : Observable<TSource> {
    // return (source: Observable<TSource>) => {
        return source.pipe(
            timeout(1),
            take(1)
        );
    // };
}
