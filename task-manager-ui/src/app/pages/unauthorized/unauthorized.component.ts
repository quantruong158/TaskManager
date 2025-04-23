import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-unauthorized',
  templateUrl: './unauthorized.component.html',
  styleUrls: ['./unauthorized.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class UnauthorizedComponent {
  constructor(private router: Router, private location: Location) {}

  public goBack() {
    this.location.back();
  }

  public goToLogin() {
    this.router.navigate(['/login']);
  }
}
