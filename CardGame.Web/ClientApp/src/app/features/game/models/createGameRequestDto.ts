export interface CreateGameRequestDto{
  PlayerIds: string[];
  DeckId: string;
  TokensToWin?: number;
}
