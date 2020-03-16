import { TestBed } from '@angular/core/testing';

import { CardModelFactoryService } from './card-model-factory.service';

describe('CardModelFactoryService', () => {
  let service: CardModelFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CardModelFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
