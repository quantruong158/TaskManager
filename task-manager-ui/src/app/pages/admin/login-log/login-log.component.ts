import { Component, signal } from '@angular/core';
import {
  ColumnDef,
  GenericTableComponent,
} from '../../../shared/components/generic-table/generic-table.component';
import { LoginLog } from '../../../core/models/logging.model';
import { formatDate } from '@angular/common';
import { LoggingService } from '../../../core/services/logging.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-login-log',
  imports: [GenericTableComponent, MatButtonModule],
  templateUrl: './login-log.component.html',
  styleUrl: './login-log.component.css',
})
export class LoginLogComponent {
  private loginLog: LoginLog[] = [];
  public filteredLog = signal<LoginLog[]>([]);
  public selectedUser: LoginLog | null = null;
  public isFiltered = signal<boolean>(false);

  public columns: ColumnDef[] = [
    {
      key: 'email',
      header: 'Email',
      sortable: true,
    },
    {
      key: 'attemptIp',
      header: 'Attempt IP',
      sortable: true,
    },
    {
      key: 'isSuccess',
      header: 'Login Status',
      type: 'boolean',
      sortable: true,
      formatter: (value: boolean) => (value ? 'Success' : 'Failed'),
    },
    {
      key: 'timestamp',
      header: 'Timestamp',
      type: 'date',
      sortable: true,
      formatter: (value: string) => formatDate(value, 'medium', 'en-US'),
    },
    {
      key: 'userAgent',
      header: 'User Agent',
      sortable: false,
    },
  ];

  constructor(private loggingService: LoggingService) {
    this.getLoginLog();
  }

  private getLoginLog() {
    this.loggingService.getLoginLog().subscribe({
      next: (response: LoginLog[]) => {
        this.loginLog = response;
        this.filteredLog.set(this.loginLog);
      },
      error: (error: any) => {
        console.error('Error fetching login log:', error);
      },
    });
  }

  public handleRowClick(row: LoginLog) {
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
      this.loginLog.filter((log) => log.email === this.selectedUser!.email)
    );
  }

  public removeFilter() {
    if (!this.isFiltered()) {
      return;
    }

    this.filteredLog.set(this.loginLog);
    this.isFiltered.set(false);
  }
}
