import { Injectable } from '@angular/core';
import { BusFactoryService } from '../domain/core/bus/bus-factory.service';
import { TopicTokens } from '../domain/core/bus/topic-tokens';
import { ClientEvent } from "./ClientEvent";

@Injectable({
  providedIn: 'root'
})
export class ClientRouterService {

  constructor(private bus: BusFactoryService) { }

  public init(): void {
    this.bus.registerToReceive<ClientEvent>(TopicTokens.clientEvent, this.OnClientEvent.bind(this));
  }
  private OnClientEvent(clientEvent: ClientEvent) {
    // todo: add topic/type validation
    this.bus.publish<any>(clientEvent.topic, clientEvent.data, clientEvent.correlationId);
  }
}
