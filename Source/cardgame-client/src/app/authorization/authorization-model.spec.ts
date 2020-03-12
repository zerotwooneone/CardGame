import { AuthorizationModel } from './authorization-model';
// Http testing module and mocking controller
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

// Other imports
import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { testproperty } from 'src/pipes/testproperty';
import { Subject } from 'rxjs';

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
        .pipe(testproperty)
        .toPromise();

      const mockRequest = httpTestingController.expectOne('/login');

      mockRequest.flush({});
      const actual = await promise;

      expect(actual).toBeTruthy();
    });
  });
});
