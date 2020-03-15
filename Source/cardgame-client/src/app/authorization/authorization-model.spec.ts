import { AuthorizationModel, ILoginResponse } from './authorization-model';
// Http testing module and mocking controller
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

// Other imports
import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { testproperty } from 'src/pipes/testproperty';
import { Subject, empty } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

describe('AuthorizationModel', () => {
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ HttpClientTestingModule ]
    });

    // Inject the http service and test controller for each test
    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    // After every test, assert that there are no more pending requests.
    httpTestingController.verify();
  });
  it('should create an instance', () => {
    expect(new AuthorizationModel(httpClient)).toBeTruthy();
  });
  it('should default to authorized false', async () => {
    const model = new AuthorizationModel(httpClient);

    const actual = await model
      .authorized
      .pipe(testproperty)
      .toPromise();

    expect(actual).toBeFalse();
  });
  describe('AuthorizationModel.login', () => {
    it('should call api and return result', async () => {
      const model = new AuthorizationModel(httpClient);
      const promise = model
        .login({username: 'username', password: 'password'})
        .toPromise();

      const mockRequest = httpTestingController.expectOne('/login');

      mockRequest.flush(createSuccessfulLoginResponse());
      const actual = await promise;

      expect(actual).toBeTruthy();
    });

    it('should fail when allready authorized', async () => {
      const model = new AuthorizationModel(httpClient);
      ((model as any).authorizedSubject as Subject<boolean>).next(true);
      const promise = model
        .login({username: 'username', password: 'password'})
        .pipe(testproperty)
        .toPromise();

      httpTestingController.expectNone('/login');

      let actual = false;
      await promise
        .catch(r => {
          actual = true;
        });

      expect(actual).toBeTrue();
    });

    it('should not make multiple api calls from multiple subscriptions', async () => {
      const model = new AuthorizationModel(httpClient);
      const observable = model
        .login({username: 'username', password: 'password'});

      const promises = [
        observable.toPromise(),
        observable.toPromise()];

      const mockRequest = httpTestingController.expectOne('/login');

      mockRequest.flush(createSuccessfulLoginResponse());

      await Promise.all(promises);
    });

    it('should become authorized after successful login', async () => {
      const model = new AuthorizationModel(httpClient);

      const promise = model
        .login({ username: 'username', password: 'password' })
        .toPromise();
      httpTestingController
        .expectOne('/login')
        .flush(createSuccessfulLoginResponse());
      await promise;

      const actual = await model
        .authorized
        .pipe(testproperty)
        .toPromise();

      expect(actual).toBeTrue();
    });

    it('should become unauthorized after error', async () => {
      const model = new AuthorizationModel(httpClient);

      const promise = model
        .login({ username: 'username', password: 'password' })
        .pipe(catchError(c => empty()))
        .toPromise();

      (model as any).authorizedSubject.next(true);

      httpTestingController
        .expectOne('/login')
        .error(new ErrorEvent('error type'));

      await promise;
      const actual = await model
        .authorized
        .pipe(testproperty)
        .toPromise();

      expect(actual).toBeFalse();
    });
  });
});

function createSuccessfulLoginResponse(): ILoginResponse {
  return {
    Success: true,
    ErrorCode: 0
  };
}
