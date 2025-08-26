import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
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
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface TemplateField {
  fieldName: string;
  displayName: string;
  dataType: 'string' | 'number' | 'date' | 'boolean' | 'email';
  defaultValue?: string;
  validationRules?: string;
  helpText?: string;
  isRequired: boolean;
  placeholder?: string;
}

export interface LetterTemplate {
  id?: string;
  name: string;
  letterType: string;
  templateContent: string;
  description?: string;
  dataSource: 'Upload' | 'Database';
  databaseQuery?: string;
  sortOrder: number;
  isActive: boolean;
  fields: TemplateField[];
}

@Component({
  selector: 'app-template-editor',
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
    MatExpansionModule,
    MatDividerModule
  ],
  template: `
    <div class="template-editor">
      <mat-card class="editor-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>edit</mat-icon>
            {{ isEditMode ? 'Edit Template' : 'Create New Template' }}
          </mat-card-title>
          <mat-card-subtitle>
            Design your letter template with dynamic fields and rich content
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <form [formGroup]="templateForm" (ngSubmit)="onSubmit()">
            <!-- Basic Information -->
            <div class="form-section">
              <h3>Basic Information</h3>
              <div class="form-row">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Template Name</mat-label>
                  <input matInput formControlName="name" placeholder="e.g., Transfer Letter Template">
                  <mat-error *ngIf="templateForm.get('name')?.hasError('required')">
                    Template name is required
                  </mat-error>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Letter Type</mat-label>
                  <input matInput formControlName="letterType" placeholder="e.g., Transfer, Experience">
                  <mat-error *ngIf="templateForm.get('letterType')?.hasError('required')">
                    Letter type is required
                  </mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Sort Order</mat-label>
                  <input matInput type="number" formControlName="sortOrder" min="1">
                  <mat-error *ngIf="templateForm.get('sortOrder')?.hasError('required')">
                    Sort order is required
                  </mat-error>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Description</mat-label>
                  <textarea matInput formControlName="description" rows="2" 
                           placeholder="Brief description of this template"></textarea>
                </mat-form-field>
              </div>
            </div>

            <!-- Data Source Configuration -->
            <div class="form-section">
              <h3>Data Source Configuration</h3>
              <div class="form-row">
                <mat-form-field appearance="outline" class="half-width">
                  <mat-label>Data Source</mat-label>
                  <mat-select formControlName="dataSource">
                    <mat-option value="Upload">Excel Upload</mat-option>
                    <mat-option value="Database">Database Query</mat-option>
                  </mat-select>
                </mat-form-field>

                <div class="half-width" *ngIf="templateForm.get('dataSource')?.value === 'Database'">
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Database Query</mat-label>
                    <textarea matInput formControlName="databaseQuery" rows="3" 
                             placeholder="SQL query to fetch data (will be configured by admin)"></textarea>
                    <mat-hint>This will be configured by your system administrator</mat-hint>
                  </mat-form-field>
                </div>
              </div>
            </div>

            <!-- Template Content -->
            <div class="form-section">
              <h3>Template Content</h3>
              <div class="content-editor">
                <div class="editor-toolbar">
                  <button type="button" mat-icon-button (click)="insertField('EmployeeName')" 
                          matTooltip="Insert Employee Name field">
                    <mat-icon>person</mat-icon>
                  </button>
                  <button type="button" mat-icon-button (click)="insertField('Department')" 
                          matTooltip="Insert Department field">
                    <mat-icon>business</mat-icon>
                  </button>
                  <button type="button" mat-icon-button (click)="insertField('Designation')" 
                          matTooltip="Insert Designation field">
                    <mat-icon>work</mat-icon>
                  </button>
                  <button type="button" mat-icon-button (click)="insertField('Date')" 
                          matTooltip="Insert Date field">
                    <mat-icon>event</mat-icon>
                  </button>
                  <button type="button" mat-icon-button (click)="insertField('Signature')" 
                          matTooltip="Insert Signature field">
                    <mat-icon>draw</mat-icon>
                  </button>
                  <button type="button" mat-icon-button (click)="showFieldManager()" 
                          matTooltip="Manage custom fields">
                    <mat-icon>settings</mat-icon>
                  </button>
                </div>

                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Template Content</mat-label>
                  <textarea matInput formControlName="templateContent" rows="15" 
                           placeholder="Write your letter template here. Use ${FieldName} for dynamic fields."
                           class="template-textarea"></textarea>
                  <mat-hint>
                    Use ${FieldName} syntax for dynamic fields. Example: Dear ${EmployeeName}, you have been transferred to ${Department}.
                  </mat-hint>
                </mat-form-field>
              </div>
            </div>

            <!-- Dynamic Fields -->
            <div class="form-section">
              <h3>Dynamic Fields</h3>
              <div class="fields-container">
                <div class="fields-header">
                  <span>Configure the fields that will be populated from your data source</span>
                  <button type="button" mat-mini-fab color="primary" (click)="addField()">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>

                <div formArrayName="fields" class="fields-list">
                  <div *ngFor="let field of fieldsArray.controls; let i = index" 
                       [formGroupName]="i" class="field-item">
                    <div class="field-row">
                      <mat-form-field appearance="outline" class="field-name">
                        <mat-label>Field Name</mat-label>
                        <input matInput formControlName="fieldName" placeholder="e.g., EmployeeName">
                      </mat-form-field>

                      <mat-form-field appearance="outline" class="field-display">
                        <mat-label>Display Name</mat-label>
                        <input matInput formControlName="displayName" placeholder="e.g., Employee Name">
                      </mat-form-field>

                      <mat-form-field appearance="outline" class="field-type">
                        <mat-label>Data Type</mat-label>
                        <mat-select formControlName="dataType">
                          <mat-option value="string">Text</mat-option>
                          <mat-option value="number">Number</mat-option>
                          <mat-option value="date">Date</mat-option>
                          <mat-option value="boolean">Yes/No</mat-option>
                          <mat-option value="email">Email</mat-option>
                        </mat-select>
                      </mat-form-field>

                      <mat-checkbox formControlName="isRequired" class="field-required">
                        Required
                      </mat-checkbox>

                      <button type="button" mat-icon-button color="warn" (click)="removeField(i)">
                        <mat-icon>delete</mat-icon>
                      </button>
                    </div>

                    <div class="field-row">
                      <mat-form-field appearance="outline" class="field-default">
                        <mat-label>Default Value</mat-label>
                        <input matInput formControlName="defaultValue" placeholder="Optional default value">
                      </mat-form-field>

                      <mat-form-field appearance="outline" class="field-help">
                        <mat-label>Help Text</mat-label>
                        <input matInput formControlName="helpText" placeholder="Help text for users">
                      </mat-form-field>

                      <mat-form-field appearance="outline" class="field-validation">
                        <mat-label>Validation Rules</mat-label>
                        <input matInput formControlName="validationRules" placeholder="e.g., min:3, max:50">
                      </mat-form-field>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Template Preview -->
            <div class="form-section">
              <h3>Template Preview</h3>
              <div class="preview-container">
                <div class="preview-header">
                  <span>Preview with sample data</span>
                  <button type="button" mat-button color="primary" (click)="generatePreview()">
                    <mat-icon>refresh</mat-icon>
                    Refresh Preview
                  </button>
                </div>
                <div class="preview-content" [innerHTML]="previewContent"></div>
              </div>
            </div>

            <!-- Actions -->
            <div class="form-actions">
              <button type="button" mat-button (click)="onCancel()">
                <mat-icon>cancel</mat-icon>
                Cancel
              </button>
              <button type="button" mat-button (click)="saveAsDraft()">
                <mat-icon>save</mat-icon>
                Save as Draft
              </button>
              <button type="submit" mat-raised-button color="primary" [disabled]="templateForm.invalid">
                <mat-icon>check</mat-icon>
                {{ isEditMode ? 'Update Template' : 'Create Template' }}
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styleUrls: ['./template-editor.component.scss']
})
export class TemplateEditorComponent implements OnInit {
  @Input() template?: LetterTemplate;
  @Input() isEditMode = false;
  @Output() templateSaved = new EventEmitter<LetterTemplate>();
  @Output() templateCancelled = new EventEmitter<void>();

  templateForm: FormGroup;
  previewContent = '';
  sampleData: any = {};

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.templateForm = this.fb.group({
      name: ['', Validators.required],
      letterType: ['', Validators.required],
      description: [''],
      dataSource: ['Upload', Validators.required],
      databaseQuery: [''],
      sortOrder: [1, [Validators.required, Validators.min(1)]],
      isActive: [true],
      templateContent: ['', Validators.required],
      fields: this.fb.array([])
    });
  }

  ngOnInit() {
    if (this.template) {
      this.loadTemplate(this.template);
    } else {
      this.addDefaultFields();
    }
    this.generatePreview();
  }

  get fieldsArray() {
    return this.templateForm.get('fields') as FormArray;
  }

  loadTemplate(template: LetterTemplate) {
    this.templateForm.patchValue({
      name: template.name,
      letterType: template.letterType,
      description: template.description,
      dataSource: template.dataSource,
      databaseQuery: template.databaseQuery,
      sortOrder: template.sortOrder,
      isActive: template.isActive,
      templateContent: template.templateContent
    });

    // Clear existing fields and add template fields
    while (this.fieldsArray.length !== 0) {
      this.fieldsArray.removeAt(0);
    }

    template.fields.forEach(field => {
      this.addField(field);
    });
  }

  addDefaultFields() {
    const defaultFields: TemplateField[] = [
      { fieldName: 'EmployeeName', displayName: 'Employee Name', dataType: 'string', isRequired: true },
      { fieldName: 'Department', displayName: 'Department', dataType: 'string', isRequired: true },
      { fieldName: 'Designation', displayName: 'Designation', dataType: 'string', isRequired: false },
      { fieldName: 'Date', displayName: 'Date', dataType: 'date', isRequired: true },
      { fieldName: 'Signature', displayName: 'Digital Signature', dataType: 'string', isRequired: true }
    ];

    defaultFields.forEach(field => this.addField(field));
  }

  addField(field?: TemplateField) {
    const fieldGroup = this.fb.group({
      fieldName: [field?.fieldName || '', Validators.required],
      displayName: [field?.displayName || '', Validators.required],
      dataType: [field?.dataType || 'string', Validators.required],
      defaultValue: [field?.defaultValue || ''],
      validationRules: [field?.validationRules || ''],
      helpText: [field?.helpText || ''],
      isRequired: [field?.isRequired || false],
      placeholder: [field?.placeholder || '']
    });

    this.fieldsArray.push(fieldGroup);
  }

  removeField(index: number) {
    this.fieldsArray.removeAt(index);
    this.generatePreview();
  }

  insertField(fieldName: string) {
    const currentContent = this.templateForm.get('templateContent')?.value || '';
    const fieldPlaceholder = `\${${fieldName}}`;
    const newContent = currentContent + fieldPlaceholder;
    this.templateForm.patchValue({ templateContent: newContent });
    this.generatePreview();
  }

  showFieldManager() {
    // This would open a modal for advanced field management
    this.snackBar.open('Field manager coming soon!', 'OK', { duration: 3000 });
  }

  generatePreview() {
    const content = this.templateForm.get('templateContent')?.value || '';
    const fields = this.templateForm.get('fields')?.value || [];
    
    // Generate sample data
    this.sampleData = {};
    fields.forEach((field: TemplateField) => {
      switch (field.dataType) {
        case 'string':
          this.sampleData[field.fieldName] = `Sample ${field.displayName}`;
          break;
        case 'number':
          this.sampleData[field.fieldName] = '123';
          break;
        case 'date':
          this.sampleData[field.fieldName] = new Date().toLocaleDateString();
          break;
        case 'boolean':
          this.sampleData[field.fieldName] = 'Yes';
          break;
        case 'email':
          this.sampleData[field.fieldName] = 'sample@example.com';
          break;
      }
    });

    // Replace placeholders with sample data
    let preview = content;
    Object.keys(this.sampleData).forEach(key => {
      const regex = new RegExp(`\\$\\{${key}\\}`, 'g');
      preview = preview.replace(regex, this.sampleData[key]);
    });

    this.previewContent = preview.replace(/\n/g, '<br>');
  }

  onSubmit() {
    if (this.templateForm.valid) {
      const template: LetterTemplate = {
        ...this.templateForm.value,
        fields: this.templateForm.get('fields')?.value || []
      };

      if (this.template?.id) {
        template.id = this.template.id;
      }

      this.templateSaved.emit(template);
    } else {
      this.markFormGroupTouched();
    }
  }

  saveAsDraft() {
    this.templateForm.patchValue({ isActive: false });
    this.onSubmit();
  }

  onCancel() {
    this.templateCancelled.emit();
  }

  private markFormGroupTouched() {
    Object.keys(this.templateForm.controls).forEach(key => {
      const control = this.templateForm.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched();
      } else {
        control?.markAsTouched();
      }
    });
  }
}
