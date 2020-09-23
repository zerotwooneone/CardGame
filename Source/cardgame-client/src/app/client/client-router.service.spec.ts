import { TestBed } from '@angular/core/testing';

import { ClientRouterService } from './client-router.service';

describe('ClientRouterService', () => {
  let service: ClientRouterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ClientRouterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
