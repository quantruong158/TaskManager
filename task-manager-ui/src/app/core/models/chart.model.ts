export interface ChartSeries {
  name: string;
  data: number[];
  color: string;
}

export interface ChartDataResponse {
  chartType: string;
  title: string;
  labels: string[];
  series: ChartSeries[];
}
