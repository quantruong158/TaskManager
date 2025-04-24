import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatisticService } from '../../core/services/statistic.service';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ChartDataResponse } from '../../core/models/chart.model';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css',
})
export class UserDashboardComponent implements OnInit {
  private readonly baseChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      title: {
        display: true,
        text: '',
        font: { size: 16 },
      },
      legend: {
        position: 'bottom',
      },
    },
  };

  public charts = {
    status: {
      data: { labels: [], datasets: [] } as ChartData,
      options: { ...this.baseChartOptions },
      type: 'pie' as ChartType,
    },
    priority: {
      data: { labels: [], datasets: [] } as ChartData,
      options: { ...this.baseChartOptions },
      type: 'bar' as ChartType,
    },
  };

  public isLoading = true;
  public error?: string;

  constructor(private statisticService: StatisticService) {}

  ngOnInit(): void {
    this.loadTaskStats();
  }

  private loadTaskStats(): void {
    const metrics = ['status', 'priority'] as const;
    metrics.forEach((metric) => {
      this.statisticService.getTaskCount(metric).subscribe({
        next: (response) => {
          this.updateChart(metric, response);
          this.isLoading = false;
        },
        error: (error) => {
          this.error = 'Failed to load task statistics';
          this.isLoading = false;
          console.error(`Error loading ${metric} statistics:`, error);
        },
      });
    });
  }

  private updateChart(
    chartKey: 'status' | 'priority',
    stats: ChartDataResponse
  ): void {
    if (this.charts[chartKey].options?.plugins?.title) {
      this.charts[chartKey].options.plugins.title.text = stats.title;
    }

    this.charts[chartKey].data = {
      labels: stats.labels,
      datasets: stats.series.map((series) => ({
        label: series.name,
        data: series.data,
        backgroundColor: series.color,
        borderColor: series.color,
        borderWidth: 1,
      })),
    };
  }
}
