import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  GenericTableComponent,
  ColumnDef,
} from '../../../shared/components/generic-table/generic-table.component';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { User } from '../../../core/models/user.model';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { UserService } from '../../../core/services/user.service';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { CreateUserDialogComponent } from '../../../components/create-user-dialog/create-user-dialog.component';
import { PaginatedResponse } from '../../../shared/models/pagination.model';

@Component({
  selector: 'app-user-management',
  imports: [
    CommonModule,
    GenericTableComponent,
    MatButtonModule,
    MatDialogModule,
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css',
})
export class UserManagementComponent implements OnInit {
  public users = signal<User[]>([]);
  public selectedUser: User | null = null;

  public columns: ColumnDef[] = [
    { key: 'name', header: 'Name', sortable: true },
    { key: 'email', header: 'Email', sortable: true },
    {
      key: 'roles',
      header: 'Roles',
      sortable: true,
      formatter: (value: { roleId: number; roleName: string }[]) =>
        value.map((role) => role.roleName).join(', '),
    },
    {
      key: 'createdAt',
      header: 'Created At',
      type: 'date',
      sortable: true,
      formatter: (value: string) => new Date(value).toLocaleDateString(),
    },
    {
      key: 'isActive',
      header: 'Status',
      type: 'boolean',
      formatter: (value: boolean) => (value ? 'Active' : 'Inactive'),
    },
  ];

  constructor(private userService: UserService, private dialog: MatDialog) {
    this.getUsers();
  }

  ngOnInit() {}

  getUsers() {
    this.userService.getUsers().subscribe({
      next: (response: User[]) => {
        this.users.set(response);
      },
      error: (error: HttpErrorResponse) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  openCreateUserDialog(): void {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      width: '500px',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.userService.createUser(result).subscribe({
          next: () => {
            this.getUsers();
          },
          error: (error: HttpErrorResponse) => {
            console.error('Error creating user:', error);
          },
        });
      }
    });
  }

  handleRowClick(row: User) {
    if (this.selectedUser === row) {
      this.selectedUser = null;
      return;
    }

    this.selectedUser = row;
  }
}
