import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTable } from '@angular/material/table';
import {
  MatPaginatorModule,
  MatPaginator,
  PageEvent,
} from '@angular/material/paginator';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';

export interface ColumnDef {
  key: string;
  header: string;
  sortable?: boolean;
  type?: 'text' | 'date' | 'number' | 'boolean' | 'custom' | 'sequence';
  formatter?: (value: any) => string;
}

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule],
  templateUrl: './generic-table.component.html',
  styleUrl: './generic-table.component.css',
})
export class GenericTableComponent implements OnInit, OnChanges {
  @Input() data: any[] = [];
  @Input() columns: ColumnDef[] = [];
  @Input() pageSize = 10;
  @Input() pageSizeOptions: number[] = [5, 10, 25, 50, 100];
  @Input() showPaginator = true;
  @Input() showSort = true;
  @Input() totalCount = 0;

  @Output() rowClick = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<PageEvent>();
  @Output() sortChange = new EventEmitter<Sort>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatTable) table!: MatTable<any>;

  displayedColumns: string[] = [];
  dataSource: MatTableDataSource<any>;
  selectedRow: any = null;

  constructor() {
    this.dataSource = new MatTableDataSource<any>([]);
  }

  ngOnInit() {
    this.displayedColumns = [
      'sequenceNo',
      ...this.columns.map((col) => col.key),
    ];
    this.updateDataSource();
  }

  getSequenceNumber(index: number): number {
    if (this.paginator) {
      return this.paginator.pageIndex * this.paginator.pageSize + index + 1;
    }
    return index + 1;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data']) {
      this.updateDataSource();
    }
  }

  private updateDataSource() {
    this.dataSource = new MatTableDataSource(this.data);
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  public onRowClick(row: any) {
    if (this.selectedRow === row) {
      this.selectedRow = null;
    } else {
      this.selectedRow = row;
    }
    this.rowClick.emit(row);
  }

  public onPageChange(event: PageEvent) {
    console.log(event);
    this.pageChange.emit(event);
  }

  public onSortChange(event: Sort) {
    this.sortChange.emit(event);
  }

  public formatCellValue(value: any, column: ColumnDef): string {
    if (column.formatter) {
      return column.formatter(value);
    }
    if (value === null || value === undefined) {
      return '';
    }
    return value.toString();
  }
}
