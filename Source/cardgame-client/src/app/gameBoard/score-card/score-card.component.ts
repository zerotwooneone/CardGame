import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { Observable } from 'rxjs';
import { CommonKnowledgePlayer } from 'src/app/game/game-client';

@Component({
  selector: 'cgc-score-card',
  templateUrl: './score-card.component.html',
  styleUrls: ['./score-card.component.scss']
})
export class ScoreCardComponent implements OnInit {

  scores = new Map<string, number>();
  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) public data: ScoreCardInput,
    private changeDetector: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.data.playerScores.subscribe(p => {
      this.scores.set(p.id, p.score);
      this.changeDetector.markForCheck();
    });
  }

  trackPlayer(player: CommonKnowledgePlayer): string {
    return player.id;
  }
}

export interface ScoreCardInput {
  readonly playerScores: Observable<CommonKnowledgePlayer>;
}
