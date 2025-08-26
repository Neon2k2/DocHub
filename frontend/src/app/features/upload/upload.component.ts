import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MaterialModule } from '../../shared/material.module';

import { DynamicTabService } from '../../core/services/dynamic-tab.service';
import { DynamicTabDto } from '../../core/models/dynamic-tab.dto';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MaterialModule]
})
export class UploadComponent implements OnInit {
  // Form groups
  excelForm: FormGroup;
  documentForm: FormGroup;
  signatureForm: FormGroup;

  // Component state
  selectedType: 'excel' | 'document' | 'signature' | null = null;
  selectedFile: File | null = null;
  isDragOver = false;
  isUploading = false;
  uploadProgress = 0;
  isLoading = false;

  // Dynamic tabs
  dynamicTabs: DynamicTabDto[] = [];
  activeTabs: DynamicTabDto[] = [];
  selectedTab: DynamicTabDto | null = null;

  // Mock data for recent uploads
  recentUploads: any[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private dynamicTabService: DynamicTabService
  ) {
    this.excelForm = this.fb.group({
      department: ['', Validators.required],
      location: ['', Validators.required],
      notes: ['']
    });

    this.documentForm = this.fb.group({
      documentType: ['', Validators.required],
      category: ['', Validators.required],
      description: ['']
    });

    this.signatureForm = this.fb.group({
      authorityName: ['', Validators.required],
      designation: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadDynamicTabs();
    this.loadUploadHistory();
  }

  // Dynamic tabs methods
  loadDynamicTabs(): void {
    this.isLoading = true;
    this.dynamicTabService.getAllActiveTabs().subscribe({
      next: (tabs) => {
        this.dynamicTabs = tabs;
        this.activeTabs = tabs.filter(tab => tab.isActive);
        if (this.activeTabs.length > 0) {
          this.selectedTab = this.activeTabs[0];
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dynamic tabs:', error);
        this.isLoading = false;
      }
    });
  }

  selectTab(tab: DynamicTabDto): void {
    this.selectedTab = tab;
  }

  onTabChange(event: any): void {
    // Handle tab change if needed
  }

  uploadForTab(tab: DynamicTabDto, event: Event): void {
    event.stopPropagation();
    this.selectedTab = tab;
    this.selectedType = 'excel';
  }

  viewTabData(tab: DynamicTabDto, event: Event): void {
    event.stopPropagation();
    // Navigate to data view or open dialog
    console.log('View data for tab:', tab);
  }

  getTabIcon(icon: string | undefined): string {
    const iconMap: { [key: string]: string } = {
      'transfer': '🔄',
      'experience': '📋',
      'confirmation': '✅',
      'termination': '🚪',
      'default': '📄'
    };
    return iconMap[icon || 'default'] || iconMap['default'];
  }

  getDataSourceColor(dataSource: string): string {
    return dataSource === 'Upload' ? 'primary' : 'accent';
  }

  // Upload type selection
  selectType(type: 'excel' | 'document' | 'signature'): void {
    this.selectedType = type;
    this.resetForm();
  }

  // Form methods
  resetForm(): void {
    this.excelForm.reset();
    this.documentForm.reset();
    this.signatureForm.reset();
    this.selectedFile = null;
    this.uploadProgress = 0;
  }

  // File handling
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFile = files[0];
    }
  }

  removeFile(): void {
    this.selectedFile = null;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Upload methods
  uploadExcel(tab?: DynamicTabDto): void {
    if (!this.excelForm.valid || !this.selectedFile) return;

    this.isUploading = true;
    this.uploadProgress = 0;

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('department', this.excelForm.get('department')?.value);
    formData.append('location', this.excelForm.get('location')?.value);
    formData.append('notes', this.excelForm.get('notes')?.value);
    
    if (tab) {
      formData.append('tabId', tab.id.toString());
    }

    // Simulate upload progress
    const interval = setInterval(() => {
      this.uploadProgress += 10;
      if (this.uploadProgress >= 100) {
        clearInterval(interval);
        this.uploadComplete();
      }
    }, 200);
  }

  uploadDocument(): void {
    if (!this.documentForm.valid || !this.selectedFile) return;

    this.isUploading = true;
    this.uploadProgress = 0;

    // Simulate upload progress
    const interval = setInterval(() => {
      this.uploadProgress += 10;
      if (this.uploadProgress >= 100) {
        clearInterval(interval);
        this.uploadComplete();
      }
    }, 200);
  }

  uploadSignature(): void {
    if (!this.signatureForm.valid || !this.selectedFile) return;

    this.isUploading = true;
    this.uploadProgress = 0;

    // Simulate upload progress
    const interval = setInterval(() => {
      this.uploadProgress += 10;
      if (this.uploadProgress >= 100) {
        clearInterval(interval);
        this.uploadComplete();
      }
    }, 200);
  }

  uploadComplete(): void {
    this.isUploading = false;
    this.uploadProgress = 0;
    this.resetForm();
    this.loadUploadHistory();
    
    // Show success message
    console.log('Upload completed successfully');
  }

  // Progress methods
  getProgressStatus(): string {
    if (this.uploadProgress < 30) return 'Preparing upload...';
    if (this.uploadProgress < 60) return 'Uploading file...';
    if (this.uploadProgress < 90) return 'Processing file...';
    return 'Finalizing...';
  }

  // Utility methods
  getUploadTitle(): string {
    switch (this.selectedType) {
      case 'excel': return 'Excel Data Upload';
      case 'document': return 'Document Upload';
      case 'signature': return 'Signature Upload';
      default: return 'Upload';
    }
  }

  getUploadDescription(): string {
    switch (this.selectedType) {
      case 'excel': return 'Upload employee data from Excel files for letter generation';
      case 'document': return 'Upload letter templates and supporting documents';
      case 'signature': return 'Upload digital signatures and authority stamps';
      default: return 'Select an upload type to continue';
    }
  }

  getUploadIcon(fileType: string): string {
    const iconMap: { [key: string]: string } = {
      'excel': '📊',
      'document': '📄',
      'signature': '✍️',
      'default': '📁'
    };
    return iconMap[fileType] || iconMap['default'];
  }

  // Mock data methods
  loadUploadHistory(): void {
    // Mock data for recent uploads
    this.recentUploads = [
      {
        id: 1,
        fileName: 'Employee_Data_Q4.xlsx',
        fileType: 'excel',
        status: 'completed',
        createdAt: new Date(Date.now() - 86400000) // 1 day ago
      },
      {
        id: 2,
        fileName: 'Transfer_Letter_Template.docx',
        fileType: 'document',
        status: 'completed',
        createdAt: new Date(Date.now() - 172800000) // 2 days ago
      },
      {
        id: 3,
        fileName: 'CEO_Signature.png',
        fileType: 'signature',
        status: 'completed',
        createdAt: new Date(Date.now() - 259200000) // 3 days ago
      }
    ];
  }

  // Template download
  downloadTemplate(): void {
    // Create and download a sample Excel template
    const templateData = [
      ['Employee ID', 'Name', 'Department', 'Position', 'Email', 'Phone'],
      ['EMP001', 'John Doe', 'IT', 'Developer', 'john.doe@company.com', '+1234567890'],
      ['EMP002', 'Jane Smith', 'HR', 'Manager', 'jane.smith@company.com', '+1234567891']
    ];

    let csvContent = 'data:text/csv;charset=utf-8,';
    templateData.forEach(row => {
      csvContent += row.join(',') + '\r\n';
    });

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', 'Employee_Data_Template.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  // Refresh data
  refreshData(): void {
    this.loadDynamicTabs();
    this.loadUploadHistory();
  }
}
