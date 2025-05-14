import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardType } from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-handmaid-protection-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './handmaid-protection-visualizer.component.html',
  styleUrls: ['./handmaid-protection-visualizer.component.scss']
})
export class HandmaidProtectionVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);
  protected readonly CardType = CardType;

  // This method is for when the Handmaid card display itself is clicked
  onHandmaidCardDisplayClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Handmaid);
  }
}
