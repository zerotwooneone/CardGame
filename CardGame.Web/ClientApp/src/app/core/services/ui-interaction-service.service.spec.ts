import { TestBed } from '@angular/core/testing';

import { UiInteractionService } from './ui-interaction-service.service';

describe('UiInteractionService', () => {
  let service: UiInteractionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UiInteractionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
