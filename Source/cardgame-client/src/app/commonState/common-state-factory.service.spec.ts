import { TestBed } from '@angular/core/testing';

import { CommonStateFactoryService } from './common-state-factory.service';

describe('CommonStateFactoryService', () => {
  let service: CommonStateFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CommonStateFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
