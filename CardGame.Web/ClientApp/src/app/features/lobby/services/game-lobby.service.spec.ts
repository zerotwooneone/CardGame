import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { GameLobbyService } from './game-lobby.service';

describe('GameLobbyService', () => {
  let service: GameLobbyService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(GameLobbyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
