import { TestBed } from '@angular/core/testing';

import { GameClientFactoryService } from './game-client-factory.service';

describe('GameClientFactoryService', () => {
  let service: GameClientFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameClientFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
