import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { StatusService } from '../../core/services/status.service';
import { CreateStatusRequest } from '../../core/models/status.model';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-create-status-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
  ],
  templateUrl: './create-status-dialog.component.html',
  styleUrl: './create-status-dialog.component.css',
})
export class CreateStatusDialogComponent {
  public nameControl: FormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(2),
  ]);

  public isActiveControl: FormControl = new FormControl(true);
  public orderControl: FormControl = new FormControl(0, [
    Validators.required,
    Validators.min(-9999),
    Validators.max(9999),
  ]);

  statusForm = new FormGroup({
    name: this.nameControl,
    isActive: this.isActiveControl,
    order: this.orderControl,
  });

  constructor(
    private dialogRef: MatDialogRef<CreateStatusDialogComponent>,
    private statusService: StatusService,
    private toastr: ToastrService
  ) {}

  onSubmit() {
    if (this.statusForm.valid) {
      this.statusService
        .createStatus(this.statusForm.value as CreateStatusRequest)
        .subscribe({
          next: (response) => {
            this.toastr.success('Status created successfully', 'SUCCESS');
            this.dialogRef.close(response);
          },
          error: (error) => {
            this.toastr.error('Error creating status', 'ERROR');
            console.error('Error creating status:', error);
          },
        });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
