// Data needed to configure the action modal
import {ActionModalType} from "./actionModalType";

export interface ActionModalData {
  actionType: ActionModalType;
  prompt: string; // e.g., "Choose a player to target:", "Guess a card (not Guard):"
  availablePlayers?: { id: string; name: string; isProtected: boolean }[]; // For 'select-player'
  availableCardTypes?: { value: number; name: string }[]; // For 'guess-card' (use CardType value)
  excludeCardTypeValue?: number; // e.g., Exclude Guard (value 1) for guessing
  currentPlayerId?: string; // To prevent targeting self in some cases
}
