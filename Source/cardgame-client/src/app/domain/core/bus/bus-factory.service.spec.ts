import { TestBed } from '@angular/core/testing';

import { BusFactoryService } from './bus-factory.service';

describe('BusFactoryService', () => {
  let service: BusFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BusFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
