import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

export interface TableColumn {
  key: string;
  label: string;
  type: 'text' | 'number' | 'date' | 'email' | 'phone' | 'select' | 'boolean' | 'action' | 'custom';
  sortable?: boolean;
  filterable?: boolean;
  editable?: boolean;
  required?: boolean;
  width?: string;
  minWidth?: string;
  maxWidth?: string;
  options?: { value: any; label: string }[];
  customTemplate?: string;
  formatter?: (value: any) => string;
  validator?: (value: any) => string | null;
}

export interface TableAction {
  label: string;
  icon: string;
  action: (row: any) => void;
  color?: 'primary' | 'accent' | 'warn';
  disabled?: (row: any) => boolean;
  hidden?: (row: any) => boolean;
  tooltip?: string;
}

export interface TableConfig {
  columns: TableColumn[];
  actions?: TableAction[];
  bulkActions?: TableAction[];
  selectable?: boolean;
  sortable?: boolean;
  filterable?: boolean;
  editable?: boolean;
  pagination?: boolean;
  pageSize?: number;
  pageSizeOptions?: number[];
  showSearch?: boolean;
  showExport?: boolean;
  showRefresh?: boolean;
  loading?: boolean;
  emptyMessage?: string;
  rowHeight?: string;
  stickyHeader?: boolean;
  stickyFooter?: boolean;
}

export interface TableData<T = any> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatMenuModule,
    MatTooltipModule,
    MatChipsModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatSelectModule
  ],
  template: `
    <div class="data-table-container" [class.loading]="config.loading">
      <!-- Table Header with Search and Actions -->
      <div class="table-header" *ngIf="config.showSearch || config.showExport || config.showRefresh">
        <div class="header-left">
          <!-- Search Bar -->
          <div class="search-container" *ngIf="config.showSearch">
            <mat-icon class="search-icon">search</mat-icon>
            <input
              matInput
              type="text"
              placeholder="Search..."
              [(ngModel)]="searchQuery"
              (input)="onSearchInput()"
              class="search-input"
              autocomplete="off">
          </div>
        </div>

        <div class="header-right">
          <!-- Export Button -->
          <button
            mat-button
            *ngIf="config.showExport"
            (click)="exportData()"
            class="export-btn">
            <mat-icon>download</mat-icon>
            Export
          </button>

          <!-- Refresh Button -->
          <button
            mat-icon-button
            *ngIf="config.showRefresh"
            (click)="refreshData()"
            [disabled]="config.loading"
            matTooltip="Refresh Data">
            <mat-icon>refresh</mat-icon>
          </button>
        </div>
      </div>

      <!-- Bulk Actions -->
      <div class="bulk-actions" *ngIf="config.bulkActions && selectedRows.length > 0">
        <div class="bulk-info">
          <span class="selected-count">{{ selectedRows.length }} item(s) selected</span>
        </div>
        <div class="bulk-buttons">
          <button
            mat-button
            *ngFor="let action of config.bulkActions"
            (click)="executeBulkAction(action)"
            [disabled]="action.disabled?.(selectedRows)"
            [color]="action.color || 'primary'"
            class="bulk-action-btn">
            <mat-icon>{{ action.icon }}</mat-icon>
            {{ action.label }}
          </button>
        </div>
      </div>

      <!-- Table Container -->
      <div class="table-container" [class.sticky-header]="config.stickyHeader">
        <table mat-table
               [dataSource]="displayedData"
                               matSort
               [matSortActive]="sort.active"
               [matSortDirection]="sort.direction"
               class="data-table"
               [class.editable]="config.editable && isEditMode"
               [style.--row-height]="config.rowHeight || '56px'">
          
          <!-- Selection Column -->
          <ng-container matColumnDef="select" *ngIf="config.selectable">
            <th mat-header-cell *matHeaderCellDef>
              <mat-checkbox
                (change)="onMasterToggle()"
                [checked]="selection.hasValue() && isAllSelected()"
                [indeterminate]="selection.hasValue() && !isAllSelected()"
                [aria-label]="'Select all'">
              </mat-checkbox>
            </th>
            <td mat-cell *matCellDef="let row">
              <mat-checkbox
                (click)="$event.stopPropagation()"
                (change)="onRowToggle(row)"
                [checked]="selection.isSelected(row)"
                [aria-label]="'Select row'">
              </mat-checkbox>
            </td>
          </ng-container>

          <!-- Data Columns -->
          <ng-container *ngFor="let column of visibleColumns" [matColumnDef]="column.key">
            <th mat-header-cell *matHeaderCellDef 
                [mat-sort-header]="column.sortable ? column.key : ''"
                [style.width]="column.width"
                [style.min-width]="column.minWidth"
                [style.max-width]="column.maxWidth">
              {{ column.label }}
            </th>
            <td mat-cell *matCellDef="let row; let rowIndex = index">
              <!-- Edit Mode -->
              <ng-container *ngIf="config.editable && isEditMode && column.editable && column.type !== 'action'">
                <ng-container [ngSwitch]="column.type">
                  
                  <!-- Text Input -->
                  <input *ngSwitchCase="'text'"
                         matInput
                         [formControlName]="column.key"
                         [placeholder]="column.label"
                         class="edit-input"
                         [class.error]="getFieldError(column.key)">
                  
                  <!-- Number Input -->
                  <input *ngSwitchCase="'number'"
                         matInput
                         type="number"
                         [formControlName]="column.key"
                         [placeholder]="column.label"
                         class="edit-input"
                         [class.error]="getFieldError(column.key)">
                  
                  <!-- Email Input -->
                  <input *ngSwitchCase="'email'"
                         matInput
                         type="email"
                         [formControlName]="column.key"
                         [placeholder]="column.label"
                         class="edit-input"
                         [class.error]="getFieldError(column.key)">
                  
                  <!-- Phone Input -->
                  <input *ngSwitchCase="'phone'"
                         matInput
                         type="tel"
                         [formControlName]="column.key"
                         [placeholder]="column.label"
                         class="edit-input"
                         [class.error]="getFieldError(column.key)">
                  
                  <!-- Date Input -->
                  <input *ngSwitchCase="'date'"
                         matInput
                         type="date"
                         [formControlName]="column.key"
                         [placeholder]="column.label"
                         class="edit-input"
                         [class.error]="getFieldError(column.key)">
                  
                  <!-- Select Dropdown -->
                  <mat-select *ngSwitchCase="'select'"
                             [formControlName]="column.key"
                             [placeholder]="column.label"
                             class="edit-select"
                             [class.error]="getFieldError(column.key)">
                    <mat-option *ngFor="let option of column.options" [value]="option.value">
                      {{ option.label }}
                    </mat-option>
                  </mat-select>
                  
                  <!-- Boolean Checkbox -->
                  <mat-checkbox *ngSwitchCase="'boolean'"
                               [formControlName]="column.key"
                               class="edit-checkbox">
                  </mat-checkbox>
                  
                  <!-- Custom Template -->
                  <ng-container *ngSwitchCase="'custom'"
                               [ngTemplateOutlet]="getCustomTemplate(column.customTemplate)"
                               [ngTemplateOutletContext]="{ $implicit: row, column: column, rowIndex: rowIndex }">
                  </ng-container>
                  
                  <!-- Default Display -->
                  <span *ngSwitchDefault>{{ getDisplayValue(row, column) }}</span>
                </ng-container>
              </ng-container>

              <!-- View Mode -->
              <ng-container *ngIf="!isEditMode || !column.editable || column.type === 'action'">
                <ng-container [ngSwitch]="column.type">
                  
                  <!-- Text Display -->
                  <span *ngSwitchCase="'text'">{{ getDisplayValue(row, column) }}</span>
                  
                  <!-- Number Display -->
                  <span *ngSwitchCase="'number'" class="number-cell">{{ getDisplayValue(row, column) }}</span>
                  
                  <!-- Date Display -->
                  <span *ngSwitchCase="'date'" class="date-cell">{{ formatDate(getDisplayValue(row, column)) }}</span>
                  
                  <!-- Email Display -->
                  <a *ngSwitchCase="'email'" 
                     [href]="'mailto:' + getDisplayValue(row, column)"
                     class="email-link">
                    {{ getDisplayValue(row, column) }}
                  </a>
                  
                  <!-- Phone Display -->
                  <a *ngSwitchCase="'phone'" 
                     [href]="'tel:' + getDisplayValue(row, column)"
                     class="phone-link">
                    {{ getDisplayValue(row, column) }}
                  </a>
                  
                  <!-- Boolean Display -->
                  <mat-icon *ngSwitchCase="'boolean'"
                           [class]="getDisplayValue(row, column) ? 'success-icon' : 'error-icon'">
                    {{ getDisplayValue(row, column) ? 'check_circle' : 'cancel' }}
                  </mat-icon>
                  
                  <!-- Select Display -->
                  <span *ngSwitchCase="'select'">{{ getSelectDisplayValue(row, column) }}</span>
                  
                  <!-- Action Buttons -->
                  <div *ngSwitchCase="'action'" class="action-buttons">
                    <button
                      mat-icon-button
                      *ngFor="let action of config.actions"
                      (click)="executeAction(action, row)"
                      [disabled]="action.disabled?.(row)"
                      [hidden]="action.hidden?.(row)"
                      [matTooltip]="action.tooltip || action.label"
                      [color]="action.color || 'primary'"
                      class="action-btn">
                      <mat-icon>{{ action.icon }}</mat-icon>
                    </button>
                  </div>
                  
                  <!-- Custom Template -->
                  <ng-container *ngSwitchCase="'custom'"
                               [ngTemplateOutlet]="getCustomTemplate(column.customTemplate)"
                               [ngTemplateOutletContext]="{ $implicit: row, column: column, rowIndex: rowIndex }">
                  </ng-container>
                  
                  <!-- Default Display -->
                  <span *ngSwitchDefault>{{ getDisplayValue(row, column) }}</span>
                </ng-container>
              </ng-container>
            </td>
          </ng-container>

          <!-- Row Actions Column -->
          <ng-container matColumnDef="rowActions" *ngIf="config.editable">
            <th mat-header-cell *matHeaderCellDef class="actions-header">
              Actions
            </th>
            <td mat-cell *matCellDef="let row; let rowIndex = index">
              <div class="row-actions">
                <!-- Edit/Save Toggle -->
                <button
                  mat-icon-button
                  *ngIf="!isRowEditing(rowIndex)"
                  (click)="startRowEdit(rowIndex)"
                  matTooltip="Edit Row"
                  color="primary"
                  class="edit-btn">
                  <mat-icon>edit</mat-icon>
                </button>
                
                <button
                  mat-icon-button
                  *ngIf="isRowEditing(rowIndex)"
                  (click)="saveRowEdit(rowIndex)"
                  matTooltip="Save Changes"
                  color="primary"
                  class="save-btn">
                  <mat-icon>save</mat-icon>
                </button>
                
                <!-- Cancel Edit -->
                <button
                  mat-icon-button
                  *ngIf="isRowEditing(rowIndex)"
                  (click)="cancelRowEdit(rowIndex)"
                  matTooltip="Cancel Edit"
                  color="warn"
                  class="cancel-btn">
                  <mat-icon>cancel</mat-icon>
                </button>
              </div>
            </td>
          </ng-container>
        </table>

        <!-- Loading Overlay -->
        <div class="loading-overlay" *ngIf="config.loading">
          <mat-progress-bar mode="indeterminate"></mat-progress-bar>
          <div class="loading-text">Loading...</div>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!config.loading && displayedData.length === 0">
          <mat-icon class="empty-icon">inbox</mat-icon>
          <h3 class="empty-title">No Data Available</h3>
          <p class="empty-message">{{ config.emptyMessage || 'There are no items to display.' }}</p>
        </div>
      </div>

      <!-- Pagination -->
      <mat-paginator
        *ngIf="config.pagination && tableData"
        [length]="tableData.totalCount"
        [pageSize]="config.pageSize || 10"
        [pageSizeOptions]="config.pageSizeOptions || [5, 10, 25, 50, 100]"
        [pageIndex]="tableData.pageNumber - 1"
        (page)="onPageChange($event)"
        class="table-paginator">
      </mat-paginator>

      <!-- Table Footer -->
      <div class="table-footer" *ngIf="config.stickyFooter">
        <div class="footer-info">
          <span>Total: {{ tableData?.totalCount || 0 }} items</span>
          <span *ngIf="config.pagination">
            Page {{ tableData?.pageNumber || 1 }} of {{ tableData?.totalPages || 1 }}
          </span>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent<T = any> implements OnInit, OnDestroy {
  @Input() config!: TableConfig;
  @Input() tableData?: TableData<T>;
  @Input() data: T[] = [];
  
  @Output() dataChange = new EventEmitter<T[]>();
  @Output() rowEdit = new EventEmitter<{ row: T; index: number }>();
  @Output() rowSave = new EventEmitter<{ row: T; index: number; originalRow: T }>();
  @Output() rowDelete = new EventEmitter<{ row: T; index: number }>();
  @Output() selectionChange = new EventEmitter<T[]>();
  @Output() sortChange = new EventEmitter<Sort>();
  @Output() pageChange = new EventEmitter<PageEvent>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() refresh = new EventEmitter<void>();
  @Output() export = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private destroy$ = new Subject<void>();

  // Component state
  isEditMode = false;
  editingRows = new Set<number>();
  originalRows = new Map<number, T>();
  rowForms = new Map<number, FormGroup>();
  selection = new SelectionModel<T>(true, []);
  
  // Display data
  displayedData: T[] = [];
  visibleColumns: TableColumn[] = [];
  
  // Search and filter
  searchQuery = '';
  searchSubject = new Subject<string>();
  
  // Sort
  sort: any = { active: '', direction: '' };

  ngOnInit(): void {
    this.initializeTable();
    this.setupSearch();
    this.updateDisplayedData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Initialization
  private initializeTable(): void {
    this.visibleColumns = this.config.columns.filter(col => col.type !== 'action');
    
    if (this.config.editable) {
      this.visibleColumns.push({
        key: 'rowActions',
        label: 'Actions',
        type: 'action'
      });
    }
  }

  private setupSearch(): void {
    this.searchSubject.pipe(
      takeUntil(this.destroy$),
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(query => {
      this.searchChange.emit(query);
    });
  }

  // Data management
  private updateDisplayedData(): void {
    if (this.tableData) {
      this.displayedData = this.tableData.data;
    } else {
      this.displayedData = this.data;
    }
  }

  // Search functionality
  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  // Pagination
  onPageChange(event: PageEvent): void {
    this.pageChange.emit(event);
  }

  // Sorting
  onSortChange(sort: Sort): void {
    this.sortChange.emit(sort);
  }

  // Selection
  onMasterToggle(): void {
    if (this.isAllSelected()) {
      this.selection.clear();
    } else {
      this.selection.select(...this.displayedData);
    }
    this.updateSelection();
  }

  onRowToggle(row: T): void {
    this.selection.toggle(row);
    this.updateSelection();
  }

  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.displayedData.length;
    return numSelected === numRows;
  }

  private updateSelection(): void {
    this.selectedRows = this.selection.selected;
    this.selectionChange.emit(this.selectedRows);
  }

  // Row editing
  startRowEdit(rowIndex: number): void {
    if (!this.config.editable) return;

    const row = this.displayedData[rowIndex];
    this.editingRows.add(rowIndex);
    this.originalRows.set(rowIndex, { ...row });
    
    const formGroup = this.createRowForm(row);
    this.rowForms.set(rowIndex, formGroup);
    
    this.isEditMode = true;
    this.rowEdit.emit({ row, index: rowIndex });
  }

  saveRowEdit(rowIndex: number): void {
    if (!this.config.editable) return;

    const form = this.rowForms.get(rowIndex);
    if (!form || !form.valid) return;

    const updatedRow = { ...this.displayedData[rowIndex], ...form.value };
    const originalRow = this.originalRows.get(rowIndex);
    
    this.displayedData[rowIndex] = updatedRow;
    this.dataChange.emit(this.displayedData);
    
    this.finishRowEdit(rowIndex);
    this.rowSave.emit({ row: updatedRow, index: rowIndex, originalRow: originalRow! });
  }

  cancelRowEdit(rowIndex: number): void {
    if (!this.config.editable) return;

    const originalRow = this.originalRows.get(rowIndex);
    if (originalRow) {
      this.displayedData[rowIndex] = originalRow;
      this.dataChange.emit(this.displayedData);
    }
    
    this.finishRowEdit(rowIndex);
  }

  private finishRowEdit(rowIndex: number): void {
    this.editingRows.delete(rowIndex);
    this.originalRows.delete(rowIndex);
    this.rowForms.delete(rowIndex);
    
    if (this.editingRows.size === 0) {
      this.isEditMode = false;
    }
  }

  private createRowForm(row: T): FormGroup {
    const group: any = {};
    
    this.config.columns.forEach(column => {
      if (column.editable && column.type !== 'action') {
        const value = (row as any)[column.key];
        const validators = column.required ? [Validators.required] : [];
        
                  // Custom validators will be handled separately
        
        group[column.key] = [value, validators];
      }
    });
    
    return this.fb.group(group);
  }

  // Utility methods
  isRowEditing(rowIndex: number): boolean {
    return this.editingRows.has(rowIndex);
  }

  getDisplayValue(row: T, column: TableColumn): any {
    const value = (row as any)[column.key];
    
    if (column.formatter) {
      return column.formatter(value);
    }
    
    return value;
  }

  getSelectDisplayValue(row: T, column: TableColumn): string {
    const value = (row as any)[column.key];
    const option = column.options?.find(opt => opt.value === value);
    return option?.label || value;
  }

  formatDate(date: any): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString();
  }

  getFieldError(fieldName: string): string | null {
    const form = this.rowForms.get(this.getCurrentEditingRowIndex());
    if (!form) return null;
    
    const control = form.get(fieldName);
    if (!control || !control.errors) return null;
    
    const errors = Object.keys(control.errors);
    return errors.length > 0 ? errors[0] : null;
  }

  private getCurrentEditingRowIndex(): number {
    return Array.from(this.editingRows)[0] || 0;
  }

  // Actions
  executeAction(action: TableAction, row: T): void {
    action.action(row);
  }

  executeBulkAction(action: TableAction): void {
    action.action(this.selectedRows);
  }

  // External actions
  refreshData(): void {
    this.refresh.emit();
  }

  exportData(): void {
    this.export.emit();
  }

  // Getters
  get selectedRows(): T[] {
    return this.selection.selected;
  }

  set selectedRows(rows: T[]) {
    this.selection.setSelection(...rows);
  }

  // Custom template handling
  getCustomTemplate(templateName?: string): any {
    // This would be implemented based on your template system
    return null;
  }
}

// Selection model for table rows
class SelectionModel<T> {
  private selection = new Set<T>();

  constructor(multiple: boolean, initiallySelected?: T[]) {
    if (initiallySelected) {
      initiallySelected.forEach(item => this.selection.add(item));
    }
  }

  select(...items: T[]): void {
    items.forEach(item => this.selection.add(item));
  }

  deselect(...items: T[]): void {
    items.forEach(item => this.selection.delete(item));
  }

  toggle(item: T): void {
    if (this.isSelected(item)) {
      this.deselect(item);
    } else {
      this.select(item);
    }
  }

  clear(): void {
    this.selection.clear();
  }

  isSelected(item: T): boolean {
    return this.selection.has(item);
  }

  hasValue(): boolean {
    return this.selection.size > 0;
  }

  get selected(): T[] {
    return Array.from(this.selection);
  }

  setSelection(...items: T[]): void {
    this.clear();
    this.select(...items);
  }
}

// Validators
class Validators {
  static required(control: any): { required: boolean } | null {
    return control.value ? null : { required: true };
  }
}
