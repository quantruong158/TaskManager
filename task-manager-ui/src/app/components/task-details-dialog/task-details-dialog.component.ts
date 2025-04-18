import { Component, Inject, Input } from '@angular/core';
import {
  FormControl,
  Validators,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { TaskService } from '../../core/services/task.service';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { CreateTaskDialogComponent } from '../create-task-dialog/create-task-dialog.component';
import { Task } from '../../core/models/task.model';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CreateStatusDialogComponent } from '../create-status-dialog/create-status-dialog.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-task-details-dialog',
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
  templateUrl: './task-details-dialog.component.html',
  styleUrl: './task-details-dialog.component.css',
})
export class TaskDetailsDialogComponent {
  public task!: Task;
  public priorityLevels = ['Low', 'Medium', 'High'];

  public titleControl: FormControl = new FormControl('', [Validators.required]);
  public descriptionControl: FormControl = new FormControl('');
  public priorityControl: FormControl = new FormControl('', [
    Validators.required,
  ]);
  public assigneeControl: FormControl = new FormControl(null);

  constructor(
    private taskService: TaskService,
    private dialogRef: MatDialogRef<TaskDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: Task,
    private toastr: ToastrService
  ) {
    this.task = data;
    console.log(this.task);
    this.titleControl.setValue(this.task.title);
    this.descriptionControl.setValue(this.task.description);
    this.priorityControl.setValue(this.task.priority);
    this.assigneeControl.setValue(this.task.assignedTo);
  }

  public editTaskForm: FormGroup = new FormGroup({
    title: this.titleControl,
    description: this.descriptionControl,
    priority: this.priorityControl,
    assignedTo: this.assigneeControl,
  });

  onSubmit() {
    if (this.editTaskForm.valid) {
      this.taskService
        .updateTask(this.task.taskId, this.editTaskForm.value)
        .subscribe({
          next: (response) => {
            this.task.title = this.editTaskForm.value.title;
            this.task.description = this.editTaskForm.value.description;
            this.task.priority = this.editTaskForm.value.priority;
            this.task.assignedTo = this.editTaskForm.value.assignedTo;
            this.toastr.success('Task updated successfully!', 'SUCCESS');
            this.dialogRef.close(this.task);
          },
          error: (error) => {
            console.error('Error updating task:', error);
            this.toastr.error('Error updating task', 'ERROR');
          },
        });
    }
  }
}
