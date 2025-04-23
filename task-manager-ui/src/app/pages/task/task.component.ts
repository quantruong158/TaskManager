import { Component, OnInit, signal } from '@angular/core';
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
  CdkDrag,
  CdkDropList,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import { StatusService } from '../../core/services/status.service';
import { Status } from '../../core/models/status.model';
import { MatGridListModule } from '@angular/material/grid-list';
import { TaskService } from '../../core/services/task.service';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { CreateTaskDialogComponent } from '../../components/create-task-dialog/create-task-dialog.component';
import { ChangeTaskStatusRequest, Task } from '../../core/models/task.model';
import { ToastrService } from 'ngx-toastr';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { RouterLink } from '@angular/router';
import { TaskBoxComponent } from '../../components/task-box/task-box.component';

@Component({
  selector: 'app-task',
  imports: [
    RouterLink,
    DragDropModule,
    CdkDropList,
    CdkDrag,
    MatGridListModule,
    MatButtonModule,
    TaskBoxComponent,
  ],
  templateUrl: './task.component.html',
  styleUrl: './task.component.css',
})
export class TaskComponent {
  private tasksByStatus = new Map<number, Task[]>();
  public statuses = signal<Status[]>([]);

  constructor(
    private statusService: StatusService,
    private taskService: TaskService,
    private dialog: MatDialog,
    private toastr: ToastrService
  ) {
    this.statusService.getAllStatuses().subscribe((res) => {
      this.statuses.set(res);

      this.statuses().forEach((status) => {
        this.tasksByStatus.set(status.statusId, []);
      });

      this.organizeTasks();
    });
  }

  private organizeTasks(): void {
    this.taskService.getAllTasks().subscribe((tasks) => {
      tasks.forEach((task) => {
        const statusId = task.status.statusId;
        if (this.tasksByStatus.has(statusId)) {
          this.tasksByStatus.get(statusId)!.push(task);
        }
      });
    });
  }

  private addAndOrganizeNewTask(taskId: number): void {
    this.taskService.getTaskById(taskId).subscribe((task) => {
      const statusId = task.status.statusId;
      if (this.tasksByStatus.has(statusId)) {
        this.tasksByStatus.get(statusId)!.push(task);
      }
    });
  }

  public getTasksByStatus(statusId: number): Task[] {
    return this.tasksByStatus.get(statusId) || [];
  }

  public getConnectedLists(currentStatusId: number): string[] {
    return this.statuses()
      .filter((status) => status.statusId !== currentStatusId)
      .map((status) => 'list-' + status.statusId);
  }

  public drop(event: CdkDragDrop<Task[]>) {
    const movedTask = event.previousContainer.data[event.previousIndex];
    const newStatusId = parseInt(event.container.id.split('-')[1]);

    if (event.previousContainer === event.container) {
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      const req: ChangeTaskStatusRequest = {
        newStatusId: newStatusId,
      };

      this.taskService
        .changeTaskStatus(movedTask.taskId, req)
        .pipe(
          catchError((error: HttpErrorResponse) => {
            transferArrayItem(
              event.container.data,
              event.previousContainer.data,
              event.currentIndex,
              event.previousIndex
            );
            this.toastr.error(error.error.message, error.error.code);
            return throwError(() => new Error('try again'));
          })
        )
        .subscribe(() => {});
    }
  }

  public openCreateDialog() {
    const dialogRef = this.dialog.open(CreateTaskDialogComponent, {
      width: '500px',
    });

    dialogRef.afterClosed().subscribe((result: number | undefined) => {
      if (!result) return;
      this.addAndOrganizeNewTask(result);
      this.toastr.success('Task created successfully!', 'SUCCESS');
    });
  }
}
