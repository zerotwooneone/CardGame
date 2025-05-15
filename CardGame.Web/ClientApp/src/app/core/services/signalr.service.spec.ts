import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { SignalrService } from './signalr.service';

describe('SignalrService', () => {
  let service: SignalrService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(SignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
