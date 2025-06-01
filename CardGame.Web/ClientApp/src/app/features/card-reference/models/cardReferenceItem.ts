export interface CardReferenceItem {
  rank: number;
  name: string;
  countInDeck: number;
  effectDescription: string;
  specialNote?: string; // Optional for cards with special rules/warnings
  // imageUrl?: string; // Optional: for card art later
}
