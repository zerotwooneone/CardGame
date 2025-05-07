import { CommonModule } from '@angular/common';
import {Component, EventEmitter, inject, OnInit, Output} from '@angular/core';
import {MatListModule} from '@angular/material/list';
import {MatIconModule} from '@angular/material/icon';
import {MatDividerModule} from '@angular/material/divider';
import {MatButtonModule} from '@angular/material/button';
import {CardReferenceService} from '../../../core/services/card-reference.service';
import {CardReferenceItem} from '../../../core/models/cardReferenceItem';

@Component({
  selector: 'app-card-reference-sheet',
  standalone: true,
  imports: [
    CommonModule,
    MatListModule,
    MatIconModule,
    MatDividerModule,
    MatButtonModule
  ],
  templateUrl: './card-reference-sheet.component.html',
  styleUrls: ['./card-reference-sheet.component.scss']
})
export class CardReferenceSheetComponent implements OnInit {
  private cardReferenceService = inject(CardReferenceService);

  @Output() closeClicked = new EventEmitter<void>();

  cardReferences: CardReferenceItem[] = [];

  ngOnInit(): void {
    this.cardReferences = this.cardReferenceService.getAllCardReferences();
  }

  onClose(): void {
    this.closeClicked.emit();
  }

  // Helper for ngFor trackBy
  trackByRank(index: number, item: CardReferenceItem): number {
    return item.rank;
  }
}
