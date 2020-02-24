import { Injectable } from '@angular/core';
import { OpenConnectionFactoryService } from '../hub/open-connection-factory.service';
import { CommonStateModel } from './common-state-model';

@Injectable({
  providedIn: 'root'
})
export class CommonStateFactoryService {

  constructor(private openConnectionFactory: OpenConnectionFactoryService) { }

  async create(gameId: string): Promise<CommonStateModel> {
    const connection = await this.openConnectionFactory.open('https://localhost:44379/commonState');
    const result = new CommonStateModel(connection);
    return result;
  }
}
