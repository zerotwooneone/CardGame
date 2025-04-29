// Data returned when the modal is closed successfully
export interface ActionModalResult {
  selectedPlayerId?: string; // For 'select-player' result
  selectedCardTypeValue?: number; // For 'guess-card' result (return CardType value)
}
