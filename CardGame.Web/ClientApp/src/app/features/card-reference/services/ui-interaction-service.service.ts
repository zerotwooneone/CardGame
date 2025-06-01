import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export interface ScrollToCardReferenceRequest {
  rank: number; // The rank of the card to scroll to
}

@Injectable({
  providedIn: 'root'
})
export class UiInteractionService {
  private scrollToCardReferenceSource = new Subject<ScrollToCardReferenceRequest>();

  /**
   * Observable that components can subscribe to for requests to scroll the card reference.
   */
  scrollToCardReference$: Observable<ScrollToCardReferenceRequest> = this.scrollToCardReferenceSource.asObservable();

  constructor() { }

  /**
   * Requests that the card reference sidenav be opened and scrolled to the specified card rank.
   * @param rank The rank of the card to scroll to.
   */
  requestScrollToCardReference(rank: number): void {
    if (rank > 0) { // Basic validation
      this.scrollToCardReferenceSource.next({ rank });
    }
  }
}
