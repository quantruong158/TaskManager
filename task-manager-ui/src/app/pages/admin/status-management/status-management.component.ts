import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import {
  ColumnDef,
  GenericTableComponent,
} from '../../../shared/components/generic-table/generic-table.component';
import { Status } from '../../../core/models/status.model';
import { StatusService } from '../../../core/services/status.service';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { HttpErrorResponse } from '@angular/common/http';
import { CreateStatusDialogComponent } from '../../../components/create-status-dialog/create-status-dialog.component';

@Component({
  selector: 'app-status-management',
  imports: [
    CommonModule,
    GenericTableComponent,
    MatButtonModule,
    MatDialogModule,
  ],
  templateUrl: './status-management.component.html',
  styleUrl: './status-management.component.css',
})
export class StatusManagementComponent {
  public statuses = signal<Status[]>([]);
  public selectedStatus: Status | null = null;

  public columns: ColumnDef[] = [
    { key: 'statusId', header: 'ID', sortable: true },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'createdAt',
      header: 'Created At',
      type: 'date',
      sortable: true,
      formatter: (value: string) => new Date(value).toLocaleDateString(),
    },
    { key: 'order', header: 'Order', sortable: true },
    {
      key: 'isActive',
      header: 'Status',
      type: 'boolean',
      formatter: (value: boolean) => (value ? 'Active' : 'Inactive'),
    },
  ];

  constructor(private statusService: StatusService, private dialog: MatDialog) {
    this.getStatuses();
  }

  getStatuses() {
    this.statusService.getAllStatuses().subscribe({
      next: (response: Status[]) => {
        this.statuses.set(response);
      },
      error: (error: HttpErrorResponse) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  handleRowClick(row: Status) {
    this.selectedStatus = row;
  }

  handlePageChange(event: PageEvent) {}

  handleSortChange(event: Sort) {
    console.log('Sort changed:', event);
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(CreateStatusDialogComponent, {
      width: '500px',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getStatuses();
      }
    });
  }
}
