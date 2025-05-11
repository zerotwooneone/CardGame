import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../card/card.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-handmaid-protection-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './handmaid-protection-visualizer.component.html',
  styleUrls: ['./handmaid-protection-visualizer.component.scss']
})
export class HandmaidProtectionVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
  private uiInteractionService = inject(UiInteractionService);

  public onCardInfoClicked(cardType: number): void {
    if (cardType) {
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
