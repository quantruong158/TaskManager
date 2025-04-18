import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Observable } from 'rxjs';
import { UserInfo } from '../../core/models/auth.model';
import { HttpClient } from '@angular/common/http';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SideBarComponent } from '../../components/side-bar/side-bar.component';

@Component({
  selector: 'app-authorized',
  templateUrl: './authorized.component.html',
  styleUrls: ['./authorized.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    SideBarComponent,
    RouterOutlet,
    RouterLink,
  ],
})
export class AuthorizedComponent implements OnInit {
  currentUser$!: Observable<UserInfo | null>;
  isExpanded = true;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.currentUser$ = this.authService.currentUser$;
  }

  toggleSidenav() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
