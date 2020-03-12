import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Subject, Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export class AuthorizationModel {
    private readonly authorizedSubject: BehaviorSubject<boolean>;
    public readonly authorized: Observable<boolean>;
    private readonly defaultHttpOptions = {
        headers: new HttpHeaders({
            'Content-Type':  'application/json',
        })
    };
    constructor(private httpClient: HttpClient) {
        this.authorizedSubject = new BehaviorSubject<boolean>(false);
        this.authorized = this.authorizedSubject.asObservable();
     }

     login(param: ILoginParam): Observable<ILoginResponse> {
        if (this.authorizedSubject.value) {
            return throwError('allready authorized');
        }
        return this
            .httpClient
            .post<IApiLoginResponse>('/login', param, this.defaultHttpOptions)
            .pipe(
                map(this.mapLogin),
                catchError(this.handleError)
                );
     }
     private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
          // A client-side or network error occurred. Handle it accordingly.
          console.error('An error occurred:', error.error.message);
        } else {
          // The backend returned an unsuccessful response code.
          // The response body may contain clues as to what went wrong,
          console.error(
            `Backend returned code ${error.status}, ` +
            `body was: ${error.error}`);
        }
        // return an observable with a user-facing error message
        return throwError(
          'Something bad happened; please try again later.');
      }
      private mapLogin(api: IApiLoginResponse): ILoginResponse {
        return api as any as ILoginResponse; // todo: this is a hack
      }
}

export interface ILoginParam {
    readonly username: string;
    readonly password: string;
}

interface IApiLoginResponse {

}

interface ILoginResponse {

}
