import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';
import { CardId } from '../card/card-id';

export class CardPlayed {
  constructor(readonly gameId: GameId,
              readonly player: PlayerId,
              readonly card: CardId) { }
}


