import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import {
  ReactiveFormsModule,
  FormGroup,
  Validators,
  FormControl,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { User } from '../../core/models/user.model';

@Component({
  selector: 'app-update-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
  ],
  templateUrl: './update-user-dialog.component.html',
  styleUrls: ['./update-user-dialog.component.css'],
})
export class UpdateUserDialogComponent {
  availableRoles = [
    { roleId: 1, roleName: 'User' },
    { roleId: 2, roleName: 'Admin' },
  ];

  constructor(
    private dialogRef: MatDialogRef<UpdateUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) private data: User
  ) {
    this.nameControl.setValue(data.name);
    this.emailControl.setValue(data.email);
    this.roleIdsControl.setValue(data.roles.map((role) => role.roleId));
    this.isActiveControl.setValue(data.isActive);

    // Disable the fields that shouldn't be editable
    this.nameControl.disable();
    this.emailControl.disable();
  }

  public nameControl: FormControl = new FormControl('', [Validators.required]);
  public emailControl: FormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);
  public roleIdsControl: FormControl = new FormControl(
    [],
    [Validators.required]
  );
  public isActiveControl: FormControl = new FormControl(true);

  updateUserForm: FormGroup = new FormGroup({
    name: this.nameControl,
    email: this.emailControl,
    roleIds: this.roleIdsControl,
    isActive: this.isActiveControl,
  });

  onSubmit(): void {
    if (this.updateUserForm.valid) {
      const result = {
        ...this.updateUserForm.value,
        name: this.data.name,
        email: this.data.email,
      };
      this.dialogRef.close(result);
    }
  }
}
