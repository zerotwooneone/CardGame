import { TestBed } from '@angular/core/testing';

import { AuthorizationModelFactoryService } from './authorization-model-factory.service';

describe('AuthorizationModelFactoryService', () => {
  let service: AuthorizationModelFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthorizationModelFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
