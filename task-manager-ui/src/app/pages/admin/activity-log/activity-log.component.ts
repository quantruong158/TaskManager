import { Component, signal } from '@angular/core';
import { ActivityLog, UserLog } from '../../../core/models/logging.model';
import { ColumnDef, GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
import { formatDate } from '@angular/common';
import { LoggingService } from '../../../core/services/logging.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-activity-log',
  imports: [GenericTableComponent, MatButtonModule],
  templateUrl: './activity-log.component.html',
  styleUrl: './activity-log.component.css',
})
export class ActivityLogComponent {
  private activityLog: ActivityLog[] = [];
  public filteredLog = signal<ActivityLog[]>([]);
  public selectedUser: ActivityLog | null = null;
  public isFiltered = signal<boolean>(false);

  public columns: ColumnDef[] = [
    {
      key: 'user',
      header: 'User',
      sortable: true,
      formatter: (value: UserLog) => value.email,
    },
    {
      key: 'action',
      header: 'Action',
      sortable: true,
    },
    {
      key: 'targetTable',
      header: 'Table',
      sortable: true,
    },
    {
      key: 'targetId',
      header: 'Id',
      sortable: true,
    },
    {
      key: 'timestamp',
      header: 'Timestamp',
      type: 'date',
      sortable: true,
      formatter: (value: string) => formatDate(value, 'medium', 'en-US'),
    },
  ];

  constructor(private loggingService: LoggingService) {
    this.getActivityLog();
  }

  private getActivityLog() {
    this.loggingService.getActivityLog().subscribe({
      next: (response: ActivityLog[]) => {
        this.activityLog = response;
        this.filteredLog.set(this.activityLog);
      },
      error: (error: any) => {
        console.error('Error fetching login log:', error);
      },
    });
  }

  public handleRowClick(row: ActivityLog) {
    if (this.selectedUser === row) {
      this.selectedUser = null;
      return;
    }

    this.selectedUser = row;
  }

  public filter() {
    if (!this.selectedUser) {
      return;
    }
    this.isFiltered.set(true);
    this.filteredLog.set(
      this.activityLog.filter(
        (log) => log.user.userId === this.selectedUser!.user.userId
      )
    );
  }

  public removeFilter() {
    if (!this.isFiltered()) {
      return;
    }

    this.filteredLog.set(this.activityLog);
    this.isFiltered.set(false);
  }
}
