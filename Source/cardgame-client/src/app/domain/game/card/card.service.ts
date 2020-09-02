import { Injectable } from '@angular/core';
import { BusFactoryService } from '../../core/bus/bus-factory.service';
import { TopicTokens } from '../../core/bus/topic-tokens';
import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';
import { CardId } from './card-id';
import { Card, PlayContext } from './card';
import { Response } from '../../core/entity/response';
import { CardStrength } from './CardStrength';
import { CardPlayed } from '../event/CardPlayed';
import { PlayRequested } from '../event/PlayRequested';
import { TradeHandsRequest } from '../event/TradeHandsRequest';
import { HandsTraded } from '../event/HandsTraded';
import { GetOtherPlayerRequest } from '../event/GetOtherPlayerRequest';
import { GetOtherPlayerAndCardGuessResponse } from '../event/GetOtherPlayerAndCardGuessResponse';
import { GetOtherPlayerAndCardGuessRequest } from '../event/GetOtherPlayerAndCardGuessRequest';
import { GetTargetPlayerRequest } from '../event/GetTargetPlayerRequest';
import { GetTargetPlayerResponse } from '../event/GetTargetPlayerResponse';
import { GetOtherPlayerResponse } from '../event/GetOtherPlayerResponse';
import { ProtectRequest } from '../event/ProtectRequest';
import { Protected } from '../event/Protected';
import { GetHandRequest } from '../event/GetHandRequest';
import { HandResponse } from '../event/HandResponse';
import { Eliminated } from '../event/Eliminated';
import { EliminateRequest } from '../event/EliminateRequest';
import { DiscardAndDrawRequest } from '../event/DiscardAndDrawRequest';
import { DiscardAndDrawed } from '../event/DiscardAndDrawed';
import { DiscardRequest } from '../event/DiscardRequest';
import { Discarded } from '../event/Discarded';

@Injectable({
  providedIn: 'root'
})
export class CardService {
  constructor(protected readonly busFactory: BusFactoryService) { }

  public onPlayRequest(request: PlayRequested, correlationId?: string) {
    // todo: pull the card factory out into a service registered by a token so it can be replaced in D.I.
    const card = Card.Factory(request.card as CardId);
    if (!card.success) {
      throw new Error(`failed to create card: ${card.reason}`);
    }
    if (!correlationId) {
      throw new Error('no correlation id on play request');
    }
    const playContext = this.createPlayContext(request.player, request.gameId, request.card, correlationId);
    (card.value as Card).Play(playContext)
      .then(
        p => this.onPlaySuccess(p, correlationId, request.card, request.gameId, request.player),
        p => this.onPlayError(p, correlationId, request.card, request.gameId, request.player));
  }
  protected onPlaySuccess(param: any,
                          correlationId: string,
                          card: CardId,
                          gameId: GameId,
                          player: PlayerId): void {
    const event = {
      gameId,
      player,
      card
    };
    const newId = 'play event success'; // todo: generate unique id
    this.busFactory.publish<CardPlayed>(TopicTokens.cardPlayedToken, event,
      newId,
      correlationId);
  }
  protected onPlayError(reason: any,
                        correlationId: string,
                        card: CardId,
                        gameId: GameId,
                        player: PlayerId): void {
    throw new Error('need to handle play errors'); // todo: need to publish error
  }
  protected createPlayContext(player: PlayerId,
                              gameId: GameId,
                              card: CardId,
                              correlationId: string): PlayContext {
    return {
      player,
      discardAndDraw: p => this.discardAndDraw(p, gameId, correlationId),
      eliminate: p => this.eliminate(p, gameId, correlationId),
      getHand: p => this.getHand(p, gameId, correlationId),
      protect: p => this.protect(p, gameId, correlationId),
      getOtherPlayer: p => this.getOtherPlayer(p, gameId, correlationId),
      getTargetPlayer: () => this.getTargetPlayer(gameId, correlationId),
      getOtherPlayerAndCardGuess: p => this.getOtherPlayerAndCardGuess(p, gameId, correlationId),
      tradeHands: p => this.tradeHands(player, p, gameId, correlationId),
      announceCardPlayed: () => this.onCardPlayed(gameId, player, card),
      announceDiscard: () => this.onDiscard(gameId, player, card),
    };
  }
  protected async onDiscard(gameId: GameId, player: PlayerId, card: CardId): Promise<any> {
    const event: DiscardRequest = {
      gameId,
      player,
      card,
    };
    await this.busFactory
      .awaitResponse<DiscardRequest, Discarded>(
        TopicTokens.discardRequest,
        TopicTokens.discardResponse,
        event,
        correlationId);
  }
  protected async onCardPlayed(gameId: GameId, player: PlayerId, card: CardId): Promise<any> {
    const event: PlayRequest = {
      gameId,
      player,
      card,
    };
    await this.busFactory
      .awaitResponse<PlayRequested, CardPlayed>(
        TopicTokens.PlayRequested,
        TopicTokens.cardPlayedToken,
        event,
        correlationId
      );
  }
  tradeHands(player1: PlayerId,
             player2: PlayerId,
             gameId: GameId,
             correlationId: string): Promise<any> {
    const event: TradeHandsRequest = {
      player1,
      player2,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<TradeHandsRequest, HandsTraded>(
        TopicTokens.tradeHandsRequest,
        TopicTokens.handsTraded,
        event,
        correlationId);
    return obs;
  }
  protected getOtherPlayerAndCardGuess(player: PlayerId,
                                       gameId: GameId,
                                       correlationId: string): Promise<Response<{ target: PlayerId, strength: CardStrength}>> {
    const event: GetOtherPlayerRequest = {
      player,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<GetOtherPlayerAndCardGuessRequest, GetOtherPlayerAndCardGuessResponse>(
        TopicTokens.getOtherPlayerAndCardGuessRequest,
        TopicTokens.getOtherPlayerAndCardGuessResponse,
        event, correlationId);
    return obs.then(e => this.mapToResponse({ target: e.target, strength: e.strength}));
  }
  protected getTargetPlayer(gameId: GameId,
                            correlationId: string): Promise<Response<PlayerId>> {
    const event: GetTargetPlayerRequest = {
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<GetTargetPlayerRequest, GetTargetPlayerResponse>(
        TopicTokens.getTargetPlayerRequest,
        TopicTokens.getTargetPlayerResponse,
        event, correlationId);
    return obs.then(e => this.mapToResponse(e.target));
  }
  protected getOtherPlayer(player: PlayerId,
                           gameId: GameId,
                           correlationId: string): Promise<Response<PlayerId>> {
    const event: GetOtherPlayerRequest = {
      player,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<GetOtherPlayerRequest, GetOtherPlayerResponse>(
        TopicTokens.getOtherPlayerRequest,
        TopicTokens.getOtherPlayerResponse,
        event, correlationId);
    return obs.then(e => this.mapToResponse(e.target));
  }
  protected mapToResponse<T>(value: T): Response<T> {
    // todo: create real responses
    return { aborted: false, value };
  }
  protected protect(target: PlayerId,
                    gameId: GameId,
                    correlationId: string): Promise<any> {
    const event: ProtectRequest = {
      target,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<ProtectRequest, Protected>(
        TopicTokens.protectRequest,
        TopicTokens.protectResponse,
        event, correlationId);
    return obs;
  }
  protected getHand(target: PlayerId,
                    gameId: GameId,
                    correlationId: string): Promise<CardId> {
    const event: GetHandRequest = {
      target,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<GetHandRequest, HandResponse>(
        TopicTokens.getHandRequest,
        TopicTokens.getHandResponse,
        event, correlationId);
    return obs.then(r => r.card);
  }
  protected eliminate(target: PlayerId,
                      gameId: GameId,
                      correlationId: string): Promise<any> {
    const event = {
      target,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<EliminateRequest, Eliminated>(
        TopicTokens.eliminateRequest,
        TopicTokens.eliminated,
        event, correlationId);
    return obs;
  }
  protected discardAndDraw(target: PlayerId,
                           gameId: GameId,
                           correlationId: string): Promise<any> {
    const event: DiscardAndDrawRequest = {
      target,
      gameId
    };
    const obs = this.busFactory
      .awaitResponse<DiscardAndDrawRequest, DiscardAndDrawed>(
        TopicTokens.discardAndDrawRequest,
        TopicTokens.discardAndDrawResponse,
        event, correlationId);
    return obs;
  }
}


