import { EventPayload } from './common-event';
import { TopicTokens } from './topic-tokens';
import { PlayRequested } from '../../game/event/PlayRequested';
import { GetOtherPlayerAndCardGuessRequest } from '../../game/event/GetOtherPlayerAndCardGuessRequest';
import { GetOtherPlayerAndCardGuessResponse } from '../../game/event/GetOtherPlayerAndCardGuessResponse';
import { GetTargetPlayerRequest } from '../../game/event/GetTargetPlayerRequest';
import { GetTargetPlayerResponse } from '../../game/event/GetTargetPlayerResponse';
import { TradeHandsRequest } from '../../game/event/TradeHandsRequest';
import { HandsTraded } from '../../game/event/HandsTraded';
import { GetOtherPlayerRequest } from '../../game/event/GetOtherPlayerRequest';
import { GetOtherPlayerResponse } from '../../game/event/GetOtherPlayerResponse';
import { ProtectRequest } from '../../game/event/ProtectRequest';
import { Protected } from '../../game/event/Protected';
import { GetHandRequest } from '../../game/event/GetHandRequest';
import { HandResponse } from '../../game/event/HandResponse';
import { EliminateRequest } from '../../game/event/EliminateRequest';
import { Eliminated } from '../../game/event/Eliminated';
import { DiscardAndDrawRequest } from '../../game/event/DiscardAndDrawRequest';
import { DiscardAndDrawed } from '../../game/event/DiscardAndDrawed';
import { ClientEvent } from 'src/app/client/client-factory.service';
import { CommonGameStateChanged } from "src/app/commonState/CommonGameStateChanged";

export class EventMap {
  private static readonly mappers: { [token: string]: MapDefinition; } = {
    [TopicTokens.PlayRequested]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        player: event?.[PayloadKeys.playerId],
        card: event?.[PayloadKeys.cardId]
      } as PlayRequested),
      requestMap: (event: PlayRequested, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.playerId]: event.player,
        [PayloadKeys.cardId]: event.card,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof PlayRequested
    },
    [TopicTokens.getOtherPlayerAndCardGuessRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        player: event?.[PayloadKeys.playerId]
      } as GetOtherPlayerAndCardGuessRequest),
      requestMap: (event: GetOtherPlayerAndCardGuessRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.playerId]: event.player,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetOtherPlayerAndCardGuessRequest
    },
    [TopicTokens.getOtherPlayerAndCardGuessResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
        strength: event?.[PayloadKeys.cardStrength]
      } as GetOtherPlayerAndCardGuessResponse),
      requestMap: (event: GetOtherPlayerAndCardGuessResponse, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        [PayloadKeys.cardStrength]: event.strength,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetOtherPlayerAndCardGuessResponse
    },
    [TopicTokens.getTargetPlayerRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
      } as GetTargetPlayerRequest),
      requestMap: (event: GetTargetPlayerRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetTargetPlayerRequest
    },
    [TopicTokens.getTargetPlayerResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target]
      } as GetTargetPlayerResponse),
      requestMap: (event: GetTargetPlayerResponse, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetTargetPlayerResponse
    },
    [TopicTokens.tradeHandsRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        player1: event?.[PayloadKeys.player1],
        player2: event?.[PayloadKeys.player2]
      } as TradeHandsRequest),
      requestMap: (event: TradeHandsRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.player1]: event.player1,
        [PayloadKeys.player2]: event.player2,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof TradeHandsRequest
    },
    [TopicTokens.handsTraded]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        player1: event?.[PayloadKeys.player1],
        player2: event?.[PayloadKeys.player2]
      } as HandsTraded),
      requestMap: (event: HandsTraded, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.player1]: event.player1,
        [PayloadKeys.player2]: event.player2,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof HandsTraded
    },
    [TopicTokens.getOtherPlayerRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        player: event?.[PayloadKeys.playerId],
      } as GetOtherPlayerRequest),
      requestMap: (event: GetOtherPlayerRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.playerId]: event.player,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetOtherPlayerRequest
    },
    [TopicTokens.getOtherPlayerResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as GetOtherPlayerResponse),
      requestMap: (event: GetOtherPlayerResponse, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetOtherPlayerResponse
    },
    [TopicTokens.protectRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as ProtectRequest),
      requestMap: (event: ProtectRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof ProtectRequest
    },
    [TopicTokens.protectResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as Protected),
      requestMap: (event: Protected, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof Protected
    },
    [TopicTokens.getHandRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as GetHandRequest),
      requestMap: (event: GetHandRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof GetHandRequest
    },
    [TopicTokens.getHandResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        card: event?.[PayloadKeys.cardId],
      } as HandResponse),
      requestMap: (event: HandResponse, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.cardId]: event.card,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof HandResponse
    },
    [TopicTokens.eliminateRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as EliminateRequest),
      requestMap: (event: EliminateRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof EliminateRequest
    },
    [TopicTokens.eliminated]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as Eliminated),
      requestMap: (event: Eliminated, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof Eliminated
    },
    [TopicTokens.discardAndDrawRequest]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as DiscardAndDrawRequest),
      requestMap: (event: DiscardAndDrawRequest, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof DiscardAndDrawRequest
    },
    [TopicTokens.discardAndDrawResponse]: {
      receiveMap: (type: string, event: EventPayload) => ({
        gameId: event?.[PayloadKeys.gameId],
        target: event?.[PayloadKeys.target],
      } as DiscardAndDrawed),
      requestMap: (event: DiscardAndDrawed, eventId?: string, correlationId?: string) => ({
        [PayloadKeys.gameId]: event.gameId,
        [PayloadKeys.target]: event.target,
        eventId,
        correlationId,
        type: typeof event
      } as EventPayload),
      type: typeof DiscardAndDrawed
    },
    [TopicTokens.clientEvent]: EventMap.GetJsonMapDefinition(typeof ClientEvent),
    [TopicTokens.GameStateChanged]: EventMap.GetJsonMapDefinition(typeof CommonGameStateChanged),
  };
  static GetJsonMapDefinition(type: string): MapDefinition {
    return {
      type,
      receiveMap: (t, ep) => {
        const x = ((ep as EventPayload).JSON as { value: string });
        return JSON.parse(x.value);
      },
      requestMap: (e, eid, cid) => ({
        type,
        correlationId: cid,
        eventId: eid,
        JSON: { value: JSON.stringify(e) }
      } as EventPayload)
    };
  }
  public Receive<T>(token: string, type: string, event?: EventPayload): T {
    const mapper = EventMap.mappers[token];
    if (!mapper) {
      // todo: do something
    }
    return mapper.receiveMap?.(type, event) as T;
  }
  public Transmit<T>(event: T,
                     token: string,
                     eventId: string,
                     correlationId: string): EventPayload | undefined {
    const mapper = EventMap.mappers[token];
    if (!mapper) {
      // todo: do something
    }
    return mapper.requestMap?.(event, eventId, correlationId);
  }
}

export enum PayloadKeys {
  gameId = 'Game Id',
  playerId = 'Player Id',
  cardId = 'Card Id',
  target = 'target',
  player1 = 'player1',
  player2 = 'player2',
  cardStrength = 'card strength'
}

export interface MapDefinition {
    receiveMap?: (type: string, e?: EventPayload) => {};
    requestMap?: (e: {}, eventId?: string, correlationId?: string) => EventPayload;
    type: string;
}
