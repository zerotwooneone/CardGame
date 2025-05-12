import { GameLogEventType } from './gameLogEventType';
import { CardType } from './cardType';
import { GameLogPlayerRoundSummaryDto } from './gameLogPlayerRoundSummaryDto';

export interface GameLogEntryDto {
  id: string;
  timestamp: Date;
  eventType: GameLogEventType;
  eventTypeName: string;
  actingPlayerId?: string;
  actingPlayerName: string;
  targetPlayerId?: string;
  targetPlayerName?: string;
  revealedCardAppearanceId?: string;
  revealedCardValue?: CardType;
  isPrivate: boolean;
  message?: string;

  // Structured properties
  playedCardAppearanceId?: string;
  playedCardValue?: CardType;

  guessedCardAppearanceId?: string;
  guessedCardValue?: CardType;
  wasGuessCorrect?: boolean;

  player1ComparedCardAppearanceId?: string;
  player1ComparedCardValue?: CardType;
  player2ComparedCardAppearanceId?: string;
  player2ComparedCardValue?: CardType;
  baronLoserPlayerId?: string;

  discardedByPrinceCardAppearanceId?: string;
  discardedByPrinceCardValue?: CardType;

  cardResponsibleForEliminationAppearanceId?: string;
  cardResponsibleForEliminationValue?: CardType;

  cardDrawnAppearanceId?: string;
  cardDrawnValue?: CardType;

  fizzleReason?: string;

  winnerPlayerId?: string;
  roundEndReason?: string;
  tokensHeld?: number;
  
  roundPlayerSummaries?: GameLogPlayerRoundSummaryDto[];
}
