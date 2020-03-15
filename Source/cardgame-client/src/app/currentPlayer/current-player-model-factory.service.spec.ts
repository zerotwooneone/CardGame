import { TestBed } from '@angular/core/testing';

import { CurrentPlayerModelFactoryService } from './current-player-model-factory.service';

describe('CurrentPlayerModelFactoryService', () => {
  let service: CurrentPlayerModelFactoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CurrentPlayerModelFactoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
