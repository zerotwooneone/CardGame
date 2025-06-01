import { TestBed } from '@angular/core/testing';

import { CardReferenceService } from './card-reference.service';

describe('CardReferenceService', () => {
  let service: CardReferenceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CardReferenceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
