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

  private getStatuses() {
    this.statusService.getAllStatuses().subscribe({
      next: (response: Status[]) => {
        this.statuses.set(response);
      },
      error: (error: HttpErrorResponse) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  public handleRowClick(row: Status) {
    if (this.selectedStatus === row) {
      this.selectedStatus = null;
      return;
    }

    this.selectedStatus = row;
  }

  public handlePageChange(event: PageEvent) {}

  public handleSortChange(event: Sort) {
    console.log('Sort changed:', event);
  }

  public openCreateDialog() {
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
