import { CardModel } from './card-model';

describe('CardModel', () => {
  it('should create an instance', () => {
    expect(new CardModel('some card id', 8)).toBeTruthy();
  });
});
