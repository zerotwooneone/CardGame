import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Subject, Observable, BehaviorSubject, throwError, from, of } from 'rxjs';
import { catchError, map, shareReplay, tap } from 'rxjs/operators';

export class AuthorizationModel {
    private readonly authorizedSubject: BehaviorSubject<boolean>;
    private readonly userTokenSubject: Subject<string>;
    private readonly refreshUserTokenSubject: Subject<string>;
    public readonly authorized: Observable<boolean>;
    private readonly defaultHttpOptions = {
        headers: new HttpHeaders({
            'Content-Type':  'application/json',
        })
    };
    constructor(private httpClient: HttpClient) {
        this.authorizedSubject = new BehaviorSubject<boolean>(false);
        this.authorized = this.authorizedSubject.asObservable();
        this.userTokenSubject = new Subject<string>();
        this.refreshUserTokenSubject = new Subject<string>();
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
                catchError(this.handleError),
                shareReplay(),
                tap(this.onLoginAttempt.bind(this))
                );
     }
  onLoginAttempt(response: ILoginResponse): void {
    this.authorizedSubject.next(response.Success);
  }
     private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
          // A client-side or network error occurred. Handle it accordingly.
          console.error('An error occurred:', error.error.message);
          const clientSideErorCode = 1;
          return of<ILoginResponse>({
            Success: false,
            ErrorCode: clientSideErorCode,
            TechnicalErrorReason: error.error.message
          });
        } else {
          // The backend returned an unsuccessful response code.
          // The response body may contain clues as to what went wrong,
          console.error(
            `Backend returned code ${error.status}, ` +
            `body was: ${error.error}`);
          return of<ILoginResponse>({
            Success: false,
            ErrorCode: error.status,
            TechnicalErrorReason: error.error
          });
        }
      }
      private mapLogin(api: IApiLoginResponse): ILoginResponse {
        if (!api) {
          return { Success: false, ErrorCode: 412 };
        }
        return api as any as ILoginResponse; // todo: this is a hack
      }
}

export interface ILoginParam {
    readonly username: string;
    readonly password: string;
}

interface IApiLoginResponse {
  readonly UserToken: string;
  readonly RefreshUserToken: string;
}

export interface ILoginResponse {
  readonly Success: boolean;
  readonly ErrorCode: number;
  readonly TechnicalErrorReason?: string;
}
