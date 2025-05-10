import { GameLogEventType } from './gameLogEventType';

export interface GameLogEntryDto {
    id: string; // Guid
    timestamp: string; // DateTimeOffset as string
    eventType: GameLogEventType;
    eventTypeName: string; // Pre-computed on server
    actingPlayerId: string; // Guid
    actingPlayerName: string;
    targetPlayerId?: string | null; // Guid
    targetPlayerName?: string | null;
    revealedCardId?: string | null; // Guid
    revealedCardType?: number | null; // CardType enum value from CardDto (e.g. 1 for Guard)
    revealedCardName?: string | null; // Pre-computed on server
    isPrivate: boolean; // Should mostly be false for logs received by SpectatorGameState
    message?: string | null;
}
