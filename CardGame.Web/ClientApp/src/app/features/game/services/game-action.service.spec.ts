import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { GameActionService } from './game-action.service';

describe('GameActionService', () => {
  let service: GameActionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(GameActionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
