import { Component, Input } from '@angular/core';
import { Task } from '../../core/models/task.model';
import {
  FormControl,
  Validators,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TaskService } from '../../core/services/task.service';
import { TaskDetailsDialogComponent } from '../task-details-dialog/task-details-dialog.component';
import { ZeroPadPipe } from '../../shared/pipes/zero-pad.pipe';

@Component({
  selector: 'task-box',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    ZeroPadPipe,
  ],
  templateUrl: './task-box.component.html',
  styleUrl: './task-box.component.css',
})
export class TaskBoxComponent {
  @Input() task!: Task;

  constructor(private dialog: MatDialog) {}

  public openDetailsDialog() {
    const dialogRef = this.dialog.open(TaskDetailsDialogComponent, {
      minWidth: '950px',
      minHeight: 'auto',
      data: this.task,
    });

    dialogRef.afterClosed().subscribe((result) => {});
  }
}
