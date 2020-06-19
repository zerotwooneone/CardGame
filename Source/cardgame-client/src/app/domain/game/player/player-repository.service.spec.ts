import { TestBed } from '@angular/core/testing';

import { PlayerRepositoryService } from './player-repository.service';

describe('PlayerRepositoryService', () => {
  let service: PlayerRepositoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlayerRepositoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
