import { TestBed } from '@angular/core/testing';

import { GameModelFactoryService } from './game-model-factory.service';

describe('GameModelFactoryService', () => {
  let service: GameModelFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameModelFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
