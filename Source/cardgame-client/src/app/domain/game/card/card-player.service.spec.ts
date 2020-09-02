import { TestBed } from '@angular/core/testing';

import { CardPlayerService } from './card-player.service';

describe('CardPlayerService', () => {
  let service: CardPlayerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CardPlayerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
