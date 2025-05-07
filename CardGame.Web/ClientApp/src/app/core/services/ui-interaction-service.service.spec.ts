import { TestBed } from '@angular/core/testing';

import { UiInteractionServiceService } from './ui-interaction-service.service';

describe('UiInteractionServiceService', () => {
  let service: UiInteractionServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UiInteractionServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
