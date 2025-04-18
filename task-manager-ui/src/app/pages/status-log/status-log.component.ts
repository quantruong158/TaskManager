import { Component, signal } from '@angular/core';
import {
  ColumnDef,
  GenericTableComponent,
} from '../../shared/components/generic-table/generic-table.component';
import { TaskStatusLog } from '../../core/models/logging.model';
import { LoggingService } from '../../core/services/logging-service';
import { formatDate } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-status-log',
  imports: [GenericTableComponent, MatButtonModule, RouterLink],
  templateUrl: './status-log.component.html',
  styleUrl: './status-log.component.css',
})
export class StatusLogComponent {
  private statusLog: TaskStatusLog[] = [];
  public filteredLog = signal<TaskStatusLog[]>([]);
  public selectedTask: TaskStatusLog | null = null;
  public isFiltered = signal<boolean>(false);

  public columns: ColumnDef[] = [
    {
      key: 'task',
      header: 'Title',
      sortable: true,
      formatter: (value: { taskId: number; title: string }) =>
        value.title + ' #' + value.taskId.toString().padStart(4, '0'),
    },
    {
      key: 'changedAt',
      header: 'Changed At',
      type: 'date',
      sortable: true,
      formatter: (value: string) => formatDate(value, 'medium', 'en-US'),
    },
    {
      key: 'changedBy',
      header: 'Changed By',
      sortable: false,
    },
    {
      key: 'status',
      header: 'New Status',
      sortable: false,
      formatter: (value: { statusId: number; statusName: string }) =>
        value.statusName,
    },
  ];

  constructor(private loggingService: LoggingService) {
    this.getStatusHistory();
  }

  private getStatusHistory() {
    this.loggingService.getTaskStatusLog().subscribe({
      next: (response: TaskStatusLog[]) => {
        this.statusLog = response;
        this.filteredLog.set(response);
      },
      error: (error: any) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  public handleRowClick(row: TaskStatusLog) {
    if (this.selectedTask === row) {
      this.selectedTask = null;
      return;
    }

    this.selectedTask = row;
  }

  public filter() {
    if (!this.selectedTask) {
      return;
    }
    this.isFiltered.set(true);
    this.filteredLog.set(
      this.statusLog.filter(
        (log) => log.task.taskId === this.selectedTask!.task.taskId
      )
    );
  }

  public removeFilter() {
    if (!this.isFiltered()) {
      return;
    }

    this.filteredLog.set(this.statusLog);
    this.isFiltered.set(false);
  }
}
