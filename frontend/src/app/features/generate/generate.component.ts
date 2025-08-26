import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { MaterialModule } from '../../shared/material.module';

import { ApiService, ApiResponse } from '../../core/services/api.service';

interface LetterTemplate {
  id: string;
  name: string;
  letterType: string;
  templateContent: string;
  dataSource: 'Upload' | 'Database';
  isActive: boolean;
  placeholders: string[];
  createdAt: Date;
}

interface Employee {
  id: string;
  employeeId: string;
  firstName: string;
  lastName: string;
  name: string;
  department: string;
  designation: string;
  email: string;
  isActive: boolean;
}

interface DigitalSignature {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureImage: string;
  isActive: boolean;
}

interface GeneratedLetter {
  id: string;
  letterNumber: string;
  letterType: string;
  employeeId: string;
  employeeName: string;
  templateId: string;
  templateName: string;
  status: 'Generated' | 'Sent' | 'Failed';
  createdAt: Date;
  sentAt?: Date;
}

@Component({
  selector: 'app-generate',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MaterialModule]
})
export class GenerateComponent implements OnInit, OnDestroy {
  @ViewChild('paginator') paginator: any;

  private destroy$ = new Subject<void>();

  // Component state
  isLoading = false;
  currentStep = 0;
  selectedLetterType = '';
  selectedTemplate: LetterTemplate | null = null;
  selectedEmployees: Employee[] = [];
  selectedSignature: DigitalSignature | null = null;
  generatedLetters: GeneratedLetter[] = [];

  // Forms
  letterForm: FormGroup;
  employeeSelectionForm: FormGroup;
  signatureForm: FormGroup;

  // Data sources
  letterTemplates: LetterTemplate[] = [];
  employees: Employee[] = [];
  digitalSignatures: DigitalSignature[] = [];

  // Pagination
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService
  ) {
    this.letterForm = this.fb.group({
      letterType: ['', Validators.required],
      template: ['', Validators.required],
      customFields: this.fb.array([])
    });

    this.employeeSelectionForm = this.fb.group({
      selectAll: [false],
      selectedEmployees: this.fb.array([])
    });

    this.signatureForm = this.fb.group({
      useStoredSignature: [true],
      generateNewSignature: [false],
      authorityName: [''],
      authorityDesignation: ['']
    });
  }

  ngOnInit(): void {
    this.loadInitialData();
    this.setupFormListeners();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Data loading
  loadInitialData(): void {
    this.isLoading = true;
    
    forkJoin({
      templates: this.apiService.getLetterTemplates(),
      employees: this.apiService.getEmployees(),
      signatures: this.apiService.getDigitalSignatures()
    }).subscribe({
      next: (data) => {
        this.letterTemplates = data.templates.data || [];
        this.employees = (data.employees.data as any)?.data || data.employees.data || [];
        this.digitalSignatures = data.signatures.data || [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        this.isLoading = false;
      }
    });
  }

  setupFormListeners(): void {
    this.letterForm.get('letterType')?.valueChanges.subscribe(letterType => {
      this.selectedLetterType = letterType;
      this.filterTemplatesByType(letterType);
    });

    this.letterForm.get('template')?.valueChanges.subscribe(templateId => {
      this.selectedTemplate = this.letterTemplates.find(t => t.id === templateId) || null;
      this.updateCustomFields();
    });

    // Listen to employee selection changes
    this.employeeSelectionForm.get('selectedEmployees')?.valueChanges.subscribe(selectedIds => {
      this.selectedEmployees = this.employees.filter(emp => selectedIds.includes(emp.id));
    });

    // Listen to signature form changes
    this.signatureForm.get('useStoredSignature')?.valueChanges.subscribe(useStored => {
      if (useStored && this.digitalSignatures.length > 0) {
        this.selectedSignature = this.digitalSignatures[0];
      } else {
        this.selectedSignature = null;
      }
    });
  }

  // Template management
  filterTemplatesByType(letterType: string): void {
    // Filter templates based on letter type
    console.log('Filtering templates for:', letterType);
  }

  updateCustomFields(): void {
    if (this.selectedTemplate) {
      const customFieldsArray = this.letterForm.get('customFields') as FormArray;
      customFieldsArray.clear();
      
      this.selectedTemplate.placeholders.forEach(placeholder => {
        customFieldsArray.push(this.fb.group({
          fieldName: [placeholder],
          value: ['', Validators.required]
        }));
      });
    }
  }

  // Employee selection
  toggleSelectAll(): void {
    const selectAll = this.employeeSelectionForm.get('selectAll')?.value;
    const selectedEmployeesArray = this.employeeSelectionForm.get('selectedEmployees') as FormArray;
    
    if (selectAll) {
      this.employees.forEach(employee => {
        if (!selectedEmployeesArray.value.includes(employee.id)) {
          selectedEmployeesArray.push(this.fb.control(employee.id));
        }
      });
    } else {
      selectedEmployeesArray.clear();
    }
  }

  toggleEmployeeSelection(employeeId: string): void {
    const selectedEmployeesArray = this.employeeSelectionForm.get('selectedEmployees') as FormArray;
    const index = selectedEmployeesArray.value.indexOf(employeeId);
    
    if (index > -1) {
      selectedEmployeesArray.removeAt(index);
    } else {
      selectedEmployeesArray.push(this.fb.control(employeeId));
    }
  }

  isEmployeeSelected(employeeId: string): boolean {
    const selectedEmployeesArray = this.employeeSelectionForm.get('selectedEmployees') as FormArray;
    return selectedEmployeesArray.value.includes(employeeId);
  }

  // Signature management
  onSignatureTypeChange(): void {
    const useStored = this.signatureForm.get('useStoredSignature')?.value;
    const generateNew = this.signatureForm.get('generateNewSignature')?.value;
    
    if (useStored && generateNew) {
      this.signatureForm.patchValue({ generateNewSignature: false });
    }
  }

  // Letter generation
  generateLetters(): void {
    if (!this.letterForm.valid || this.selectedEmployees.length === 0) {
      return;
    }

    this.isLoading = true;
    
    // Simulate letter generation
    setTimeout(() => {
      this.generatedLetters = this.selectedEmployees.map(employee => ({
        id: Date.now().toString() + Math.random(),
        letterNumber: `LTR-${Date.now()}`,
        letterType: this.selectedLetterType,
        employeeId: employee.id,
        employeeName: employee.name,
        templateId: this.selectedTemplate?.id || '',
        templateName: this.selectedTemplate?.name || '',
        status: 'Generated',
        createdAt: new Date()
      }));
      
      this.isLoading = false;
      this.currentStep = 3; // Move to review step
    }, 2000);
  }

  sendLetters(): void {
    console.log('Sending letters:', this.generatedLetters.length);
    // TODO: Implement email sending functionality
  }

  onLetterTypeChange(): void {
    console.log('Letter type changed:', this.selectedLetterType);
    // TODO: Implement letter type change logic
  }

  // Navigation
  nextStep(): void {
    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  // Utility methods
  getSelectedEmployeesCount(): number {
    const selectedEmployeesArray = this.letterForm.get('selectedEmployees') as FormArray;
    return selectedEmployeesArray.length;
  }

  canProceedToNextStep(): boolean {
    switch (this.currentStep) {
      case 0: return !!(this.letterForm.get('letterType')?.valid && this.letterForm.get('template')?.valid);
      case 1: return this.selectedEmployees.length > 0;
      case 2: return this.selectedSignature !== null || (this.signatureForm.get('useStoredSignature')?.value === true);
      default: return true;
    }
  }

  getStepStatus(step: number): string {
    if (step < this.currentStep) return 'completed';
    if (step === this.currentStep) return 'current';
    return 'pending';
  }
}
