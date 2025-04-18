import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-create-user-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
  ],
  templateUrl: './create-user-dialog.component.html',
  styleUrl: './create-user-dialog.component.css',
})
export class CreateUserDialogComponent {
  availableRoles = [
    { roleId: 1, roleName: 'User' },
    { roleId: 2, roleName: 'Admin' },
  ];

  constructor(private dialogRef: MatDialogRef<CreateUserDialogComponent>) {}

  public nameControl: FormControl = new FormControl('', [Validators.required]);
  public emailControl: FormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);
  public passwordControl: FormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(6),
  ]);
  public roleIdsControl: FormControl = new FormControl(
    [],
    [Validators.required]
  );

  createUserForm: FormGroup = new FormGroup({
    name: this.nameControl,
    email: this.emailControl,
    password: this.passwordControl,
    roleIds: this.roleIdsControl,
  });

  onSubmit(): void {
    if (this.createUserForm.valid) {
      this.dialogRef.close(this.createUserForm.value);
    }
  }
}
