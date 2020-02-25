import { TestBed } from '@angular/core/testing';

import { ClientFactoryService } from './client-factory.service';

describe('ClientFactoryService', () => {
  let service: ClientFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ClientFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
