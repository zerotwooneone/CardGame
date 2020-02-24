import { TestBed } from '@angular/core/testing';

import { OpenConnectionFactoryService } from './open-connection-factory.service';

describe('OpenConnectionFactoryService', () => {
  let service: OpenConnectionFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OpenConnectionFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
