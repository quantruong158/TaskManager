import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';

interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'app-side-bar',
  templateUrl: './side-bar.component.html',
  styleUrls: ['./side-bar.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatListModule,
    MatIconModule,
    MatSidenavModule,
  ],
})
export class SideBarComponent implements OnChanges {
  @Input() isExpanded = true;
  @Input() role: string | null = null;

  public navItems: NavItem[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['role']) {
      this.updateNavItems();
    }
  }

  private updateNavItems(): void {
    const commonItems: NavItem[] = [
      { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
      { icon: 'task', label: 'Tasks', route: '/task' },
      { icon: 'person', label: 'Profile', route: '/profile' },
    ];

    const adminItems: NavItem[] = [
      {
        icon: 'admin_panel_settings',
        label: 'Admin Dashboard',
        route: '/admin/dashboard',
      },
      { icon: 'people', label: 'User Management', route: '/admin/users' },
      {
        icon: 'check_circle',
        label: 'Status Management',
        route: '/admin/status',
      },
      {
        icon: 'description',
        label: 'Login Log',
        route: '/admin/login-log',
      },
      {
        icon: 'description',
        label: 'Activity Log',
        route: '/admin/activity-log',
      },
      { icon: 'settings', label: 'System Settings', route: '/admin/settings' },
    ];

    this.navItems = this.role === 'Admin' ? adminItems : commonItems;
  }
}
