import { CardId } from './card-id';
import { ValueResult } from '../../core/value/value-result';
import { CardStrength } from './CardStrength';
import { PlayerId } from '../player/player-id';
import { Princess} from './Princess';
import { Countess } from './Countess';
import { Prince } from './Prince';
import { King } from './King';
import { Handmaid } from './Handmaid';
import { Baron } from './Baron';
import { Priest } from './Priest';
import { Guard } from './Guard';
import { Response } from '../../core/entity/response';

export abstract class Card {
    public static Factory(id: CardId): ValueResult<Card> {
        if (!id) {
            return ValueResult.CreateError<Card>('id is required');
        }
        switch (id.value.value) {
            case CardStrength.Princess: {
                return ValueResult.CreateSuccess<Card>(new Princess(id));
            }
            case CardStrength.Countess: {
                return ValueResult.CreateSuccess<Card>(new Countess(id));
            }
            case CardStrength.King: {
                return ValueResult.CreateSuccess<Card>(new King(id));
            }
            case CardStrength.Prince: {
                return ValueResult.CreateSuccess<Card>(new Prince(id));
            }
            case CardStrength.Handmaid: {
                return ValueResult.CreateSuccess<Card>(new Handmaid(id));
            }
            case CardStrength.Baron: {
                return ValueResult.CreateSuccess<Card>(new Baron(id));
            }
            case CardStrength.Priest: {
                return ValueResult.CreateSuccess<Card>(new Priest(id));
            }
            case CardStrength.Guard: {
                return ValueResult.CreateSuccess<Card>(new Guard(id));
            }
            default: {
                return ValueResult.CreateError(`ucard type ${id.value.value}`);
            }
        }
    }
    protected constructor(readonly id: CardId) {}

    public GetMustPlay(other?: CardStrength): boolean {
        return false;
    }

    public async Play(playContext: PlayContext): Promise<any> {
        await this.PlayImplementation(playContext);
        await playContext.announceCardPlayed();
    }
    protected PlayImplementation(playContext: PlayContext): Promise<any> {
        return Promise.resolve({});
    }

    public async Discard(discardContext: DiscardContext): Promise<any> {
        await discardContext.announceDiscard();
    }
}

export interface DiscardContext {
    readonly player: PlayerId;
    eliminate(player: PlayerId): Promise<any>;

    // todo:move this to a seperate interface
    announceDiscard(): Promise<any>;
}

export interface PlayContext extends DiscardContext {
    getOtherPlayerAndCardGuess(player: PlayerId): Promise<Response<{ target: PlayerId, strength: CardStrength }>>;
    getHand(value: PlayerId): Promise<CardId>;
    protect(player: PlayerId): Promise<any>;
    tradeHands(player: PlayerId, value: any): Promise<any>;
    getOtherPlayer(player: PlayerId): Promise<Response<PlayerId>>;
    discardAndDraw(discardTarget: PlayerId): Promise<any>;
    getTargetPlayer(): Promise<Response<PlayerId>>;

    // todo:move this to a seperate interface
    announceCardPlayed(): Promise<any>;
}
