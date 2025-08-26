import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipListboxChange } from '@angular/material/chips';
import { Subject, takeUntil } from 'rxjs';

import { ApiService, ApiResponse } from '../../../core/services/api.service';
import { DynamicTabService } from '../../../core/services/dynamic-tab.service';
import { NotificationService } from '../../../core/services/notification.service';

interface DynamicTab {
  id: string;
  name: string;
  displayName: string;
  description: string;
  dataSource: 'Upload' | 'Database';
  databaseQuery?: string;
  icon: string;
  color: string;
  sortOrder: number;
  isActive: boolean;
  isAdminOnly: boolean;
  requiredPermission?: string;
  letterTemplates: LetterTemplate[];
}

interface LetterTemplate {
  id: string;
  name: string;
  letterType: string;
  description: string;
  templateContent: string;
  templateFilePath?: string;
  category?: string;
  dataSource: 'Upload' | 'Database';
  databaseQuery?: string;
  isActive: boolean;
  sortOrder: number;
  fields: LetterTemplateField[];
}

interface LetterTemplateField {
  id: string;
  fieldName: string;
  displayName: string;
  dataType: string;
  fieldType?: string;
  isRequired: boolean;
  defaultValue?: string;
  validationRules?: string;
  helpText?: string;
  sortOrder: number;
}

@Component({
  selector: 'app-dynamic-tab-manager',
  templateUrl: './dynamic-tab-manager.component.html',
  styleUrls: ['./dynamic-tab-manager.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDialogModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatTabsModule,
    MatExpansionModule,
    MatSlideToggleModule,
    MatTooltipModule
  ]
})
export class DynamicTabManagerComponent implements OnInit, OnDestroy {
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private destroy$ = new Subject<void>();

  // Data
  dynamicTabs: DynamicTab[] = [];
  letterTemplates: LetterTemplate[] = [];
  isLoading = false;
  isCreating = false;
  isEditing = false;

  // Forms
  tabForm: FormGroup;
  templateForm: FormGroup;
  fieldForm: FormGroup;

  // Table
  displayedColumns: string[] = [
    'name', 'displayName', 'dataSource', 'icon', 'color', 'isActive', 'actions'
  ];

  // UI State
  selectedTab: DynamicTab | null = null;
  selectedTemplate: LetterTemplate | null = null;
  activeTabIndex = 0;
  showAdvancedOptions = false;

  // Available options
  dataSourceOptions = ['Upload', 'Database'];
  dataTypeOptions = ['Text', 'Number', 'Date', 'Email', 'Phone', 'Select', 'Multiselect', 'File'];
  fieldTypeOptions = ['Input', 'Textarea', 'Select', 'Checkbox', 'Radio', 'DatePicker', 'FileUpload'];
  iconOptions = ['📄', '📝', '📋', '📤', '📧', '✍️', '🔑', '⚙️', '👥', '📊', '📈', '📉', '🎯', '💼', '🏢', '📱', '💻', '📱', '🌐', '🔒'];
  colorOptions = ['#0969da', '#1a7f37', '#9a6700', '#cf222e', '#8250df', '#116329', '#953800', '#82071e', '#1b1f23', '#656d76'];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private dynamicTabService: DynamicTabService,
    private notificationService: NotificationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadDynamicTabs();
    this.loadLetterTemplates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForms(): void {
    this.tabForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      displayName: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      dataSource: ['Upload', Validators.required],
      databaseQuery: [''],
      icon: ['📄', Validators.required],
      color: ['#0969da', Validators.required],
      sortOrder: [0, [Validators.required, Validators.min(0)]],
      isActive: [true],
      isAdminOnly: [false],
      requiredPermission: ['']
    });

    this.templateForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      letterType: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(500)]],
      templateContent: ['', Validators.required],
      templateFilePath: [''],
      category: [''],
      dataSource: ['Upload', Validators.required],
      databaseQuery: [''],
      isActive: [true],
      sortOrder: [0, [Validators.required, Validators.min(0)]],
      fields: this.fb.array([])
    });

    this.fieldForm = this.fb.group({
      fieldName: ['', [Validators.required, Validators.maxLength(100)]],
      displayName: ['', [Validators.required, Validators.maxLength(200)]],
      dataType: ['Text', Validators.required],
      fieldType: ['Input'],
      isRequired: [false],
      defaultValue: [''],
      validationRules: [''],
      helpText: [''],
      sortOrder: [0, [Validators.required, Validators.min(0)]]
    });

    // Watch for data source changes
    this.tabForm.get('dataSource')?.valueChanges.subscribe(value => {
      const databaseQueryControl = this.tabForm.get('databaseQuery');
      if (value === 'Database') {
        databaseQueryControl?.setValidators([Validators.required, Validators.maxLength(4000)]);
      } else {
        databaseQueryControl?.clearValidators();
      }
      databaseQueryControl?.updateValueAndValidity();
    });

    this.templateForm.get('dataSource')?.valueChanges.subscribe(value => {
      const databaseQueryControl = this.templateForm.get('databaseQuery');
      if (value === 'Database') {
        databaseQueryControl?.setValidators([Validators.required, Validators.maxLength(4000)]);
      } else {
        databaseQueryControl?.clearValidators();
      }
      databaseQueryControl?.updateValueAndValidity();
    });
  }

  loadDynamicTabs(): void {
    this.isLoading = true;
    this.dynamicTabService.getDynamicTabs()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.dynamicTabs = response.data;
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading dynamic tabs:', error);
          this.notificationService.showError('Failed to load dynamic tabs');
          this.isLoading = false;
        }
      });
  }

  loadLetterTemplates(): void {
    this.apiService.getLetterTemplates()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<LetterTemplate[]>) => {
          if (response.success && response.data) {
            this.letterTemplates = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading letter templates:', error);
        }
      });
  }

  onCreateTab(): void {
    this.isCreating = true;
    this.isEditing = false;
    this.selectedTab = null;
    this.tabForm.reset({
      dataSource: 'Upload',
      icon: '📄',
      color: '#0969da',
      sortOrder: 0,
      isActive: true,
      isAdminOnly: false
    });
    this.activeTabIndex = 0;
  }

  onEditTab(tab: DynamicTab): void {
    this.isEditing = true;
    this.isCreating = false;
    this.selectedTab = tab;
    this.tabForm.patchValue(tab);
    this.activeTabIndex = 0;
  }

  onDeleteTab(tab: DynamicTab): void {
    if (confirm(`Are you sure you want to delete "${tab.displayName}"? This action cannot be undone.`)) {
      this.dynamicTabService.deleteDynamicTab(tab.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.notificationService.showSuccess('Dynamic tab deleted successfully');
              this.loadDynamicTabs();
            } else {
              this.notificationService.showError(response.message || 'Failed to delete dynamic tab');
            }
          },
          error: (error) => {
            console.error('Error deleting dynamic tab:', error);
            this.notificationService.showError('Failed to delete dynamic tab');
          }
        });
    }
  }

  onSaveTab(): void {
    if (this.tabForm.valid) {
      const tabData = this.tabForm.value;
      
      if (this.isEditing && this.selectedTab) {
        // Update existing tab
        this.dynamicTabService.updateDynamicTab(this.selectedTab.id, tabData)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.notificationService.showSuccess('Dynamic tab updated successfully');
                this.loadDynamicTabs();
                this.resetForms();
              } else {
                this.notificationService.showError(response.message || 'Failed to update dynamic tab');
              }
            },
            error: (error) => {
              console.error('Error updating dynamic tab:', error);
              this.notificationService.showError('Failed to update dynamic tab');
            }
          });
      } else {
        // Create new tab
        this.dynamicTabService.createDynamicTab(tabData)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.notificationService.showSuccess('Dynamic tab created successfully');
                this.loadDynamicTabs();
                this.resetForms();
              } else {
                this.notificationService.showError(response.message || 'Failed to create dynamic tab');
              }
            },
            error: (error) => {
              console.error('Error creating dynamic tab:', error);
              this.notificationService.showError('Failed to create dynamic tab');
            }
          });
      }
    } else {
      this.markFormGroupTouched(this.tabForm);
    }
  }

  onCreateTemplate(): void {
    this.activeTabIndex = 1;
    this.templateForm.reset({
      dataSource: 'Upload',
      isActive: true,
      sortOrder: 0,
      fields: []
    });
    this.selectedTemplate = null;
  }

  onEditTemplate(template: LetterTemplate): void {
    this.activeTabIndex = 1;
    this.selectedTemplate = template;
    this.templateForm.patchValue(template);
    
    // Set up fields form array
    const fieldsArray = this.templateForm.get('fields') as FormArray;
    fieldsArray.clear();
    
    if (template.fields) {
      template.fields.forEach(field => {
        fieldsArray.push(this.fb.group({
          id: [field.id],
          fieldName: [field.fieldName, Validators.required],
          displayName: [field.displayName, Validators.required],
          dataType: [field.dataType, Validators.required],
          fieldType: [field.fieldType || 'Input'],
          isRequired: [field.isRequired],
          defaultValue: [field.defaultValue || ''],
          validationRules: [field.validationRules || ''],
          helpText: [field.helpText || ''],
          sortOrder: [field.sortOrder, Validators.required]
        }));
      });
    }
  }

  onSaveTemplate(): void {
    if (this.templateForm.valid) {
      const templateData = this.templateForm.value;
      
      if (this.selectedTemplate) {
        // Update existing template
        this.apiService.updateLetterTemplate(this.selectedTemplate.id, templateData)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (response: ApiResponse<LetterTemplate>) => {
              if (response.success) {
                this.notificationService.showSuccess('Letter template updated successfully');
                this.loadLetterTemplates();
                this.resetForms();
              } else {
                this.notificationService.showError(response.message || 'Failed to update template');
              }
            },
            error: (error) => {
              console.error('Error updating template:', error);
              this.notificationService.showError('Failed to update template');
            }
          });
      } else {
        // Create new template
        this.apiService.createLetterTemplate(templateData)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (response: ApiResponse<LetterTemplate>) => {
              if (response.success) {
                this.notificationService.showSuccess('Letter template created successfully');
                this.loadLetterTemplates();
                this.resetForms();
              } else {
                this.notificationService.showError(response.message || 'Failed to create template');
              }
            },
            error: (error) => {
              console.error('Error creating template:', error);
              this.notificationService.showError('Failed to create template');
            }
          });
      }
    } else {
      this.markFormGroupTouched(this.templateForm);
    }
  }

  addField(): void {
    const fieldsArray = this.templateForm.get('fields') as FormArray;
    fieldsArray.push(this.fb.group({
      fieldName: ['', Validators.required],
      displayName: ['', Validators.required],
      dataType: ['Text', Validators.required],
      fieldType: ['Input'],
      isRequired: [false],
      defaultValue: [''],
      validationRules: [''],
      helpText: [''],
      sortOrder: [fieldsArray.length, Validators.required]
    }));
  }

  removeField(index: number): void {
    const fieldsArray = this.templateForm.get('fields') as FormArray;
    fieldsArray.removeAt(index);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Handle file upload logic here
      this.templateForm.patchValue({
        templateFilePath: file.name
      });
    }
  }

  resetForms(): void {
    this.tabForm.reset({
      dataSource: 'Upload',
      icon: '📄',
      color: '#0969da',
      sortOrder: 0,
      isActive: true,
      isAdminOnly: false
    });
    
    this.templateForm.reset({
      dataSource: 'Upload',
      isActive: true,
      sortOrder: 0,
      fields: []
    });
    
    this.selectedTab = null;
    this.selectedTemplate = null;
    this.isCreating = false;
    this.isEditing = false;
    this.activeTabIndex = 0;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else {
        control?.markAsTouched();
      }
    });
  }

  getFieldsFormArray(): FormArray {
    return this.templateForm.get('fields') as FormArray;
  }

  getTabDataSourceIcon(dataSource: string): string {
    return dataSource === 'Upload' ? '📤' : '🗄️';
  }

  getTabStatusColor(isActive: boolean): string {
    return isActive ? 'primary' : 'warn';
  }
}
