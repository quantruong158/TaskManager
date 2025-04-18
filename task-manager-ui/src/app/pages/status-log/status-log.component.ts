import { Component, signal } from '@angular/core';
import {
  ColumnDef,
  GenericTableComponent,
} from '../../shared/components/generic-table/generic-table.component';
import { TaskStatusHistory } from '../../core/models/task.model';
import { TaskStatusHistoryService } from '../../core/services/task-status-history.service';
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
  private statusLog: TaskStatusHistory[] = [];
  public filteredLog = signal<TaskStatusHistory[]>([]);
  public selectedUser: TaskStatusHistory | null = null;
  public totalCount = signal<number>(0);
  public currentPage = signal<number>(1);
  public pageSize = signal<number>(10);
  public selectedTask: TaskStatusHistory | null = null;
  public isFiltered = signal<boolean>(false);

  public columns: ColumnDef[] = [
    // { key: 'historyId', header: 'ID', sortable: true },
    {
      key: 'task',
      header: 'Title',
      sortable: true,
      formatter: (value: { taskId: number; title: string }) => value.title,
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

  constructor(private taskStatusHistoryService: TaskStatusHistoryService) {
    this.getStatusHistory();
  }

  private getStatusHistory() {
    this.taskStatusHistoryService.getTaskStatusHistory().subscribe({
      next: (response: TaskStatusHistory[]) => {
        this.statusLog = response;
        this.filteredLog.set(response);
      },
      error: (error: any) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  public handleRowClick(row: TaskStatusHistory) {
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
