import { TestBed } from '@angular/core/testing';

import { TurnRepositoryService } from './turn-repository.service';

describe('TurnRepositoryService', () => {
  let service: TurnRepositoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TurnRepositoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
