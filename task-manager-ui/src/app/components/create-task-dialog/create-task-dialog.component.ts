import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  ReactiveFormsModule,
  FormGroup,
  FormBuilder,
  Validators,
  FormControl,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { CreateTaskRequest } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-create-task-dialog',
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
  ],
  templateUrl: './create-task-dialog.component.html',
  styleUrl: './create-task-dialog.component.css',
})
export class CreateTaskDialogComponent {
  public priorityLevels = ['Low', 'Medium', 'High'];

  constructor(
    private taskService: TaskService,
    private dialogRef: MatDialogRef<CreateTaskDialogComponent>,
  ) {
  }

  public titleControl: FormControl = new FormControl('', [Validators.required]);
  public descriptionControl: FormControl = new FormControl('');
  public priorityControl: FormControl = new FormControl('', [
    Validators.required,
  ]);

  public createTaskForm: FormGroup = new FormGroup({
    title: this.titleControl,
    description: this.descriptionControl,
    priority: this.priorityControl,
  });

  public onSubmit() {
    if (this.createTaskForm.valid) {
      this.taskService.createTask(this.createTaskForm.value).subscribe({
        next: (response) => {
          this.dialogRef.close(response);
        },
        error: (error) => {
          console.error('Error creating task:', error);
        },
      });
    }
  }

  public onCancel() {
    this.dialogRef.close();
  }
}
