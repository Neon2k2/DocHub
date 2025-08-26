import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { DynamicTabDto, CreateDynamicTabDto, UpdateDynamicTabDto } from '../../../core/models/dynamic-tab.dto';

export interface TabDialogData {
  mode: 'create' | 'edit';
  tab?: DynamicTabDto;
}

@Component({
  selector: 'app-tab-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSlideToggleModule,
    MatDividerModule,
    MatCardModule,
    MatProgressBarModule
  ],
  template: `
    <div class="tab-dialog">
      <div class="dialog-header">
        <h2 class="dialog-title">
          <mat-icon class="title-icon">{{ isEditMode ? 'edit' : 'add' }}</mat-icon>
          {{ isEditMode ? 'Edit Tab' : 'Create New Tab' }}
        </h2>
        <p class="dialog-subtitle">
          {{ isEditMode ? 'Update tab configuration' : 'Configure a new dynamic tab for letter generation' }}
        </p>
      </div>

      <form [formGroup]="tabForm" class="tab-form">
        <div class="form-section">
          <h3 class="section-title">Basic Information</h3>
          
          <div class="form-grid">
            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Tab Name</mat-label>
              <input matInput formControlName="name" 
                     placeholder="e.g., transfer-letter" 
                     [readonly]="isEditMode"
                     required>
              <mat-hint>Unique identifier for the tab (cannot be changed)</mat-hint>
              <mat-error *ngIf="tabForm.get('name')?.hasError('required')">
                Tab name is required
              </mat-error>
              <mat-error *ngIf="tabForm.get('name')?.hasError('pattern')">
                Use lowercase letters, numbers, and hyphens only
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Display Name</mat-label>
              <input matInput formControlName="displayName" 
                     placeholder="e.g., Transfer Letter" 
                     required>
              <mat-hint>User-friendly name displayed in the interface</mat-hint>
              <mat-error *ngIf="tabForm.get('displayName')?.hasError('required')">
                Display name is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field full-width">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" 
                        rows="3" 
                        placeholder="Describe the purpose and content of this letter type"></textarea>
              <mat-hint>Optional description to help users understand this tab</mat-hint>
            </mat-form-field>
          </div>
        </div>

        <mat-divider></mat-divider>

        <div class="form-section">
          <h3 class="section-title">Data Configuration</h3>
          
          <div class="form-grid">
            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Data Source</mat-label>
              <mat-select formControlName="dataSource" required>
                <mat-option value="Upload">
                  <div class="option-content">
                    <mat-icon>upload_file</mat-icon>
                    <span>Upload</span>
                    <small>Excel files uploaded by users</small>
                  </div>
                </mat-option>
                <mat-option value="Database">
                  <div class="option-content">
                    <mat-icon>storage</mat-icon>
                    <span>Database</span>
                    <small>Existing data from legacy systems</small>
                  </div>
                </mat-option>
              </mat-select>
              <mat-hint>Choose where employee data comes from</mat-hint>
              <mat-error *ngIf="tabForm.get('dataSource')?.hasError('required')">
                Data source is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field" 
                          *ngIf="tabForm.get('dataSource')?.value === 'Database'">
              <mat-label>Database Query</mat-label>
              <textarea matInput formControlName="databaseQuery" 
                        rows="4" 
                        placeholder="SELECT * FROM employees WHERE department = @department"></textarea>
              <mat-hint>SQL query to fetch data (use @parameters for dynamic values)</mat-hint>
            </mat-form-field>
          </div>
        </div>

        <mat-divider></mat-divider>

        <div class="form-section">
          <h3 class="section-title">Visual Configuration</h3>
          
          <div class="form-grid">
            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Icon</mat-label>
              <mat-select formControlName="icon">
                <mat-option value="transfer">🔄 Transfer</mat-option>
                <mat-option value="experience">💼 Experience</mat-option>
                <mat-option value="confirmation">✅ Confirmation</mat-option>
                <mat-option value="cessation">🚪 Cessation</mat-option>
                <mat-option value="appointment">📋 Appointment</mat-option>
                <mat-option value="promotion">📈 Promotion</mat-option>
                <mat-option value="warning">⚠️ Warning</mat-option>
                <mat-option value="custom">🎨 Custom</mat-option>
              </mat-select>
              <mat-hint>Choose an icon to represent this tab</mat-hint>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Color</mat-label>
              <input matInput formControlName="color" 
                     type="color" 
                     placeholder="#3B82F6">
              <mat-hint>Choose a color theme for this tab</mat-hint>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Sort Order</mat-label>
              <input matInput formControlName="sortOrder" 
                     type="number" 
                     min="1" 
                     max="100"
                     placeholder="1">
              <mat-hint>Position in the navigation (lower numbers appear first)</mat-hint>
              <mat-error *ngIf="tabForm.get('sortOrder')?.hasError('min')">
                Sort order must be at least 1
              </mat-error>
              <mat-error *ngIf="tabForm.get('sortOrder')?.hasError('max')">
                Sort order cannot exceed 100
              </mat-error>
            </mat-form-field>
          </div>
        </div>

        <mat-divider></mat-divider>

        <div class="form-section">
          <h3 class="section-title">Access Control</h3>
          
          <div class="form-grid">
            <div class="toggle-group">
              <mat-slide-toggle formControlName="isActive" color="primary">
                Active Tab
              </mat-slide-toggle>
              <small>Enable this tab for users</small>
            </div>

            <div class="toggle-group">
              <mat-slide-toggle formControlName="isAdminOnly" color="primary">
                Admin Only
              </mat-slide-toggle>
              <small>Restrict access to administrators only</small>
            </div>

            <mat-form-field appearance="outline" class="form-field" 
                          *ngIf="tabForm.get('isAdminOnly')?.value">
              <mat-label>Required Permission</mat-label>
              <input matInput formControlName="requiredPermission" 
                     placeholder="e.g., manage-letters">
              <mat-hint>Specific permission required to access this tab</mat-hint>
            </mat-form-field>
          </div>
        </div>

        <!-- Preview Section -->
        <div class="form-section" *ngIf="tabForm.get('displayName')?.value">
          <h3 class="section-title">Preview</h3>
          
          <div class="preview-card">
            <div class="preview-header">
              <div class="preview-icon" [style.background-color]="tabForm.get('color')?.value || '#3B82F6'">
                <span>{{ getIconPreview(tabForm.get('icon')?.value) }}</span>
              </div>
              <div class="preview-info">
                <h4>{{ tabForm.get('displayName')?.value }}</h4>
                <p>{{ tabForm.get('description')?.value || 'No description provided' }}</p>
                <div class="preview-tags">
                  <mat-chip [color]="getDataSourceColor(tabForm.get('dataSource')?.value)" selected>
                    {{ tabForm.get('dataSource')?.value }}
                  </mat-chip>
                  <mat-chip [color]="tabForm.get('isActive')?.value ? 'primary' : 'warn'" selected>
                    {{ tabForm.get('isActive')?.value ? 'Active' : 'Inactive' }}
                  </mat-chip>
                  <mat-chip *ngIf="tabForm.get('isAdminOnly')?.value" color="accent" selected>
                    Admin Only
                  </mat-chip>
                </div>
              </div>
            </div>
          </div>
        </div>
      </form>

      <div class="dialog-actions">
        <button mat-button (click)="onCancel()" class="cancel-btn">
          Cancel
        </button>
        <button mat-raised-button 
                color="primary" 
                [disabled]="!tabForm.valid || isSubmitting"
                (click)="onSubmit()"
                class="submit-btn">
          <mat-icon>{{ isEditMode ? 'save' : 'add' }}</mat-icon>
          <span>{{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update Tab' : 'Create Tab') }}</span>
        </button>
      </div>

      <!-- Progress Bar -->
      <mat-progress-bar *ngIf="isSubmitting" 
                        mode="indeterminate" 
                        class="progress-bar">
      </mat-progress-bar>
    </div>
  `,
  styleUrls: ['./tab-dialog.component.scss']
})
export class TabDialogComponent implements OnInit {
  tabForm: FormGroup;
  isEditMode = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<TabDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TabDialogData
  ) {
    this.isEditMode = data.mode === 'edit';
    this.tabForm = this.fb.group({
      name: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
      displayName: ['', Validators.required],
      description: [''],
      dataSource: ['Upload', Validators.required],
      databaseQuery: [''],
      icon: ['transfer'],
      color: ['#3B82F6'],
      sortOrder: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
      isActive: [true],
      isAdminOnly: [false],
      requiredPermission: ['']
    });
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.tab) {
      this.tabForm.patchValue({
        name: this.data.tab.name,
        displayName: this.data.tab.displayName,
        description: this.data.tab.description,
        dataSource: this.data.tab.dataSource,
        databaseQuery: this.data.tab.databaseQuery,
        icon: this.data.tab.icon,
        color: this.data.tab.color,
        sortOrder: this.data.tab.sortOrder,
        isActive: this.data.tab.isActive,
        isAdminOnly: this.data.tab.isAdminOnly,
        requiredPermission: this.data.tab.requiredPermission
      });
    }

    // Set default color if not provided
    if (!this.tabForm.get('color')?.value) {
      this.tabForm.patchValue({ color: '#3B82F6' });
    }
  }

  onSubmit(): void {
    if (this.tabForm.valid) {
      this.isSubmitting = true;
      
      const formValue = this.tabForm.value;
      
      if (this.isEditMode) {
        const updateDto: UpdateDynamicTabDto = {
          displayName: formValue.displayName,
          description: formValue.description,
          dataSource: formValue.dataSource,
          databaseQuery: formValue.databaseQuery,
          icon: formValue.icon,
          color: formValue.color,
          sortOrder: formValue.sortOrder,
          isActive: formValue.isActive,
          isAdminOnly: formValue.isAdminOnly,
          requiredPermission: formValue.requiredPermission
        };
        
        this.dialogRef.close({ action: 'update', data: updateDto });
      } else {
        const createDto: CreateDynamicTabDto = {
          name: formValue.name,
          displayName: formValue.displayName,
          description: formValue.description,
          dataSource: formValue.dataSource,
          databaseQuery: formValue.databaseQuery,
          icon: formValue.icon,
          color: formValue.color,
          sortOrder: formValue.sortOrder,
          isAdminOnly: formValue.isAdminOnly,
          requiredPermission: formValue.requiredPermission
        };
        
        this.dialogRef.close({ action: 'create', data: createDto });
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close({ action: 'cancel' });
  }

  getIconPreview(icon: string): string {
    const iconMap: { [key: string]: string } = {
      'transfer': '🔄',
      'experience': '💼',
      'confirmation': '✅',
      'cessation': '🚪',
      'appointment': '📋',
      'promotion': '📈',
      'warning': '⚠️',
      'custom': '🎨'
    };
    return iconMap[icon] || '📄';
  }

  getDataSourceColor(dataSource: string): string {
    return dataSource === 'Upload' ? 'primary' : 'accent';
  }
}
