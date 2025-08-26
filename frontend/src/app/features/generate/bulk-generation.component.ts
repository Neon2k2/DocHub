import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';

export interface EmployeeData {
  id: string;
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  designation: string;
  isSelected: boolean;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  errorMessage?: string;
}

export interface BulkGenerationConfig {
  letterType: string;
  templateId: string;
  useStoredSignature: boolean;
  generateNewSignature: boolean;
  sendEmail: boolean;
  emailSubject: string;
  emailBody: string;
  attachments: string[];
}

@Component({
  selector: 'app-bulk-generation',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  template: `
    <div class="bulk-generation">
      <mat-card class="generation-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>batch_prediction</mat-icon>
            Bulk Letter Generation
          </mat-card-title>
          <mat-card-subtitle>
            Generate letters for multiple employees simultaneously
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <!-- Configuration Section -->
          <div class="config-section">
            <h3>Generation Configuration</h3>
            <form [formGroup]="configForm" class="config-form">
              <div class="form-row">
                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Letter Type</mat-label>
                  <mat-select formControlName="letterType" (selectionChange)="onLetterTypeChange()">
                    <mat-option *ngFor="let type of availableLetterTypes" [value]="type.id">
                      {{ type.name }}
                    </mat-option>
                  </mat-select>
                </mat-form-field>

                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Template</mat-label>
                  <mat-select formControlName="templateId" [disabled]="!configForm.get('letterType')?.value">
                    <mat-option *ngFor="let template of availableTemplates" [value]="template.id">
                      {{ template.name }}
                    </mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Email Subject</mat-label>
                  <input matInput formControlName="emailSubject" placeholder="Subject for generated emails">
                </mat-form-field>

                <div class="half-width signature-options">
                  <h4>Signature Options</h4>
                  <div class="checkbox-group">
                    <mat-checkbox formControlName="useStoredSignature">
                      Use stored signature
                    </mat-checkbox>
                    <mat-checkbox formControlName="generateNewSignature">
                      Generate new signature
                    </mat-checkbox>
                  </div>
                </div>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Email Body</mat-label>
                  <textarea matInput formControlName="emailBody" rows="4" 
                           placeholder="Email body content (optional)"></textarea>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-checkbox formControlName="sendEmail" class="full-width">
                  Send emails automatically after generation
                </mat-checkbox>
              </div>
            </form>
          </div>

          <!-- Employee Selection -->
          <div class="selection-section">
            <h3>Employee Selection</h3>
            <div class="selection-controls">
              <button mat-button (click)="selectAll()">
                <mat-icon>select_all</mat-icon>
                Select All
              </button>
              <button mat-button (click)="deselectAll()">
                <mat-icon>deselect</mat-icon>
                Deselect All
              </button>
              <button mat-button (click)="selectByDepartment()">
                <mat-icon>filter_list</mat-icon>
                Select by Department
              </button>
              <span class="selected-count">
                {{ selectedCount }} of {{ totalEmployees }} selected
              </span>
            </div>

            <div class="table-container">
              <table mat-table [dataSource]="employees" class="employee-table">
                <!-- Selection Column -->
                <ng-container matColumnDef="select">
                  <th mat-header-cell *matHeaderCellDef>
                    <mat-checkbox (change)="$event ? masterToggle() : null"
                                  [checked]="selection.hasValue() && isAllSelected()"
                                  [indeterminate]="selection.hasValue() && !isAllSelected()">
                    </mat-checkbox>
                  </th>
                  <td mat-cell *matCellDef="let row">
                    <mat-checkbox (click)="$event.stopPropagation()"
                                  (change)="$event ? selection.toggle(row) : null"
                                  [checked]="selection.isSelected(row)">
                    </mat-checkbox>
                  </td>
                </ng-container>

                <!-- Employee ID Column -->
                <ng-container matColumnDef="employeeId">
                  <th mat-header-cell *matHeaderCellDef>Employee ID</th>
                  <td mat-cell *matCellDef="let employee">{{ employee.employeeId }}</td>
                </ng-container>

                <!-- Name Column -->
                <ng-container matColumnDef="name">
                  <th mat-header-cell *matHeaderCellDef>Name</th>
                  <td mat-cell *matCellDef="let employee">
                    {{ employee.firstName }} {{ employee.lastName }}
                  </td>
                </ng-container>

                <!-- Department Column -->
                <ng-container matColumnDef="department">
                  <th mat-header-cell *matHeaderCellDef>Department</th>
                  <td mat-cell *matCellDef="let employee">{{ employee.department }}</td>
                </ng-container>

                <!-- Designation Column -->
                <ng-container matColumnDef="designation">
                  <th mat-header-cell *matHeaderCellDef>Designation</th>
                  <td mat-cell *matCellDef="let employee">{{ employee.designation }}</td>
                </ng-container>

                <!-- Email Column -->
                <ng-container matColumnDef="email">
                  <th mat-header-cell *matHeaderCellDef>Email</th>
                  <td mat-cell *matCellDef="let employee">{{ employee.email }}</td>
                </ng-container>

                <!-- Status Column -->
                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let employee">
                    <span class="status-badge" [class]="employee.status">
                      {{ employee.status | titlecase }}
                    </span>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                    [class.selected]="selection.isSelected(row)"
                    (click)="selection.toggle(row)">
                </tr>
              </table>

              <mat-paginator [pageSizeOptions]="[10, 25, 50, 100]" showFirstLastButtons>
              </mat-paginator>
            </div>
          </div>

          <!-- Progress Section -->
          <div class="progress-section" *ngIf="isGenerating">
            <h3>Generation Progress</h3>
            <div class="progress-info">
              <div class="progress-stats">
                <span>Completed: {{ completedCount }}</span>
                <span>Failed: {{ failedCount }}</span>
                <span>Remaining: {{ remainingCount }}</span>
              </div>
              <mat-progress-bar mode="determinate" [value]="progressPercentage" 
                               class="progress-bar"></mat-progress-bar>
              <div class="progress-text">
                {{ currentOperation }} - {{ progressPercentage | number:'1.0-0' }}%
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="action-section">
            <button mat-raised-button color="primary" 
                    [disabled]="!canStartGeneration() || isGenerating"
                    (click)="startBulkGeneration()">
              <mat-icon>play_arrow</mat-icon>
              Start Bulk Generation
            </button>

            <button mat-button color="warn" 
                    [disabled]="!isGenerating"
                    (click)="stopGeneration()">
              <mat-icon>stop</mat-icon>
              Stop Generation
            </button>

            <button mat-button (click)="exportResults()" 
                    [disabled]="completedCount === 0">
              <mat-icon>download</mat-icon>
              Export Results
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styleUrls: ['./bulk-generation.component.scss']
})
export class BulkGenerationComponent implements OnInit {
  @Input() employees: EmployeeData[] = [];
  @Input() availableLetterTypes: any[] = [];
  @Input() availableTemplates: any[] = [];
  @Output() generationStarted = new EventEmitter<BulkGenerationConfig>();
  @Output() generationStopped = new EventEmitter<void>();

  configForm: FormGroup;
  displayedColumns: string[] = ['select', 'employeeId', 'name', 'department', 'designation', 'email', 'status'];
  
  isGenerating = false;
  currentOperation = 'Preparing...';
  progressPercentage = 0;
  completedCount = 0;
  failedCount = 0;
  remainingCount = 0;

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.configForm = this.fb.group({
      letterType: ['', Validators.required],
      templateId: ['', Validators.required],
      useStoredSignature: [true],
      generateNewSignature: [false],
      sendEmail: [true],
      emailSubject: [''],
      emailBody: [''],
      attachments: [[]]
    });
  }

  ngOnInit() {
    this.remainingCount = this.employees.length;
    this.setupFormValidation();
  }

  setupFormValidation() {
    // Ensure at least one signature option is selected
    this.configForm.get('useStoredSignature')?.valueChanges.subscribe(value => {
      if (!value && !this.configForm.get('generateNewSignature')?.value) {
        this.configForm.patchValue({ generateNewSignature: true });
      }
    });

    this.configForm.get('generateNewSignature')?.valueChanges.subscribe(value => {
      if (!value && !this.configForm.get('useStoredSignature')?.value) {
        this.configForm.patchValue({ useStoredSignature: true });
      }
    });
  }

  onLetterTypeChange() {
    const letterType = this.configForm.get('letterType')?.value;
    if (letterType) {
      // Filter templates by letter type
      const filteredTemplates = this.availableTemplates.filter(t => t.letterType === letterType);
      this.availableTemplates = filteredTemplates;
      
      if (filteredTemplates.length > 0) {
        this.configForm.patchValue({ templateId: filteredTemplates[0].id });
      }
    }
  }

  selectAll() {
    this.employees.forEach(emp => emp.isSelected = true);
  }

  deselectAll() {
    this.employees.forEach(emp => emp.isSelected = false);
  }

  selectByDepartment() {
    // This would open a dialog to select departments
    this.snackBar.open('Department selection dialog coming soon!', 'OK', { duration: 3000 });
  }

  get selectedCount(): number {
    return this.employees.filter(emp => emp.isSelected).length;
  }

  get totalEmployees(): number {
    return this.employees.length;
  }

  canStartGeneration(): boolean {
    return this.configForm.valid && this.selectedCount > 0;
  }

  startBulkGeneration() {
    if (!this.canStartGeneration()) {
      this.snackBar.open('Please configure generation settings and select employees', 'OK', { duration: 3000 });
      return;
    }

    const config: BulkGenerationConfig = {
      ...this.configForm.value,
      attachments: this.configForm.get('attachments')?.value || []
    };

    this.isGenerating = true;
    this.completedCount = 0;
    this.failedCount = 0;
    this.remainingCount = this.selectedCount;
    this.progressPercentage = 0;

    // Start the generation process
    this.generationStarted.emit(config);
    this.simulateGenerationProgress();
  }

  stopGeneration() {
    this.isGenerating = false;
    this.generationStopped.emit();
    this.snackBar.open('Generation stopped by user', 'OK', { duration: 2000 });
  }

  simulateGenerationProgress() {
    const selectedEmployees = this.employees.filter(emp => emp.isSelected);
    let currentIndex = 0;

    const processNext = () => {
      if (!this.isGenerating || currentIndex >= selectedEmployees.length) {
        this.generationComplete();
        return;
      }

      const employee = selectedEmployees[currentIndex];
      this.currentOperation = `Processing ${employee.firstName} ${employee.lastName}`;
      
      // Simulate processing time
      setTimeout(() => {
        if (Math.random() > 0.1) { // 90% success rate
          employee.status = 'completed';
          this.completedCount++;
        } else {
          employee.status = 'failed';
          employee.errorMessage = 'Generation failed';
          this.failedCount++;
        }

        this.remainingCount--;
        currentIndex++;
        this.progressPercentage = ((this.completedCount + this.failedCount) / selectedEmployees.length) * 100;

        processNext();
      }, 1000 + Math.random() * 2000); // 1-3 seconds per employee
    };

    processNext();
  }

  generationComplete() {
    this.isGenerating = false;
    this.currentOperation = 'Generation Complete';
    
    const message = `Bulk generation completed! ${this.completedCount} successful, ${this.failedCount} failed.`;
    this.snackBar.open(message, 'OK', { duration: 5000 });
  }

  exportResults() {
    const results = this.employees.filter(emp => emp.status === 'completed' || emp.status === 'failed');
    
    // Create CSV content
    let csvContent = 'Employee ID,Name,Department,Designation,Email,Status,Error\n';
    results.forEach(emp => {
      csvContent += `${emp.employeeId},"${emp.firstName} ${emp.lastName}",${emp.department},${emp.designation},${emp.email},${emp.status},"${emp.errorMessage || ''}"\n`;
    });

    // Download CSV file
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `bulk_generation_results_${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);

    this.snackBar.open('Results exported successfully!', 'OK', { duration: 2000 });
  }

  // Selection handling methods
  masterToggle() {
    if (this.isAllSelected()) {
      this.deselectAll();
    } else {
      this.selectAll();
    }
  }

  isAllSelected() {
    const numSelected = this.selectedCount;
    const numRows = this.totalEmployees;
    return numSelected === numRows;
  }

  hasValue() {
    return this.selectedCount > 0;
  }
}
