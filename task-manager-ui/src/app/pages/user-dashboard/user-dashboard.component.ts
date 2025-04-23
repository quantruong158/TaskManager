import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatisticService } from '../../core/services/statistic.service';
import { TaskCountResponse } from '../../core/models/task.model';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css',
})
export class UserDashboardComponent implements OnInit {
  taskStats?: TaskCountResponse;
  isLoading = true;
  error?: string;

  constructor(private statisticService: StatisticService) {}

  ngOnInit(): void {
    this.loadTaskStats();
  }

  private loadTaskStats(): void {
    this.isLoading = true;
    this.statisticService.getTaskCount().subscribe({
      next: (response) => {
        this.taskStats = response;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load task statistics';
        this.isLoading = false;
        console.error('Error loading task statistics:', error);
      },
    });
  }
}
