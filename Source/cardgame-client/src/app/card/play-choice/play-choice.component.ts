import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CardStrength } from 'src/app/domain/card/CardStrength';

@Component({
  selector: 'cgc-play-choice',
  templateUrl: './play-choice.component.html',
  styleUrls: ['./play-choice.component.scss']
})
export class PlayChoiceComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: ChoiceInput,
    public dialogRef: MatDialogRef<PlayChoiceComponent>) { }
  players: Player[] = [
    { value: '9b644228-6c7e-4caa-becf-89e093ee299f', viewValue: 'Jeb' },
    { value: '5e96fafb-83b2-4e72-8afa-0e6a8f12345f', viewValue: 'Molly' },
  ];
  strengths: Strength[] = [
    { value: 2, viewValue: 'Priest - 2' },
    { value: 3, viewValue: 'Baron - 3' },
    { value: 4, viewValue: 'Handmaid - 4' },
    { value: 5, viewValue: 'Prince - 5' },
    { value: 6, viewValue: 'King - 6' },
    { value: 7, viewValue: 'Countess - 7' },
    { value: 8, viewValue: 'Princess - 8' },
  ];
  playerRequired = true;
  playerFormControl: FormControl;
  strengthRequired = false;
  strengthFormControl: FormControl;

  ngOnInit(): void {
    const input = this.data;

    this.playerRequired = !!input?.targetPlayers?.length;
    this.strengthRequired = input?.strengthRequired;

    this.playerFormControl = new FormControl(null, this.playerRequired ? Validators.required : null);
    this.strengthFormControl = new FormControl(null, this.strengthRequired ? Validators.required : null);
  }

  playClick(): void {
    const strength = this.strengthFormControl.value === ''
      ? null
      : parseInt(this.strengthFormControl.value, 10);
    const result: ChoiceOutput = {
      strength,
      target: this.playerFormControl.value,
    };
    this.dialogRef.close(result);
  }
}

export interface ChoiceInput {
  readonly targetPlayers: readonly string[] | null;
  readonly strengthRequired: boolean;
}

export interface ChoiceOutput {
  readonly target: string | null;
  readonly strength: CardStrength | null;
}

interface Player {
  value: string;
  viewValue: string;
}

interface Strength {
  value: CardStrength;
  viewValue: string;
}
