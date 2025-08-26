import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatListModule } from '@angular/material/list';
import { MatBadgeModule } from '@angular/material/badge';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, interval } from 'rxjs';

import { ApiService, ApiResponse } from '../../core/services/api.service';

interface DigitalSignature {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureImage: string;
  isActive: boolean;
  createdAt: Date;
  lastUsed: Date;
  usageCount: number;
  deviceInfo: DeviceInfo;
}

interface DeviceInfo {
  isConnected: boolean;
  deviceName: string;
  serialNumber: string;
  firmwareVersion: string;
  availableSignatures: number;
  lastConnected: Date;
}

interface SignatureStats {
  totalSignatures: number;
  activeSignatures: number;
  deviceConnected: boolean;
  deviceName: string;
  availableSignatures: number;
  lastSignatureDate: Date | undefined;
  lastSignatureAuthority: string;
}

@Component({
  selector: 'app-signatures',
  templateUrl: './signatures.component.html',
  styleUrls: ['./signatures.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatProgressBarModule,
    MatChipsModule,
    MatTooltipModule,
    MatExpansionModule,
    MatListModule,
    MatBadgeModule,
    ReactiveFormsModule
  ]
})
export class SignaturesComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private destroy$ = new Subject<void>();

  // Data
  signatures: DigitalSignature[] = [];
  signatureStats: SignatureStats | null = null;
  deviceInfo: DeviceInfo | null = null;
  
  // UI State
  isLoading = true;
  isGenerating = false;
  isTestingConnection = false;
  showGenerateForm = false;
  
  // Forms
  generateForm: FormGroup;
  
  // Table
  displayedColumns: string[] = [
    'authorityName',
    'authorityDesignation',
    'status',
    'createdAt',
    'lastUsed',
    'usageCount',
    'actions'
  ];
  
  // Pagination
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50];

  constructor(
    private apiService: ApiService,
    private formBuilder: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.generateForm = this.formBuilder.group({
      authorityName: ['', [Validators.required, Validators.minLength(2)]],
      authorityDesignation: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  ngOnInit(): void {
    this.loadSignatures();
    this.loadDeviceInfo();
    this.loadSignatureStats();
    
    // Set up periodic refresh every 30 seconds
    interval(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.refreshData();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSignatures(): void {
    this.isLoading = true;
    this.apiService.getDigitalSignatures()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<DigitalSignature[]>) => {
          if (response.success && response.data) {
            this.signatures = response.data.map(sig => ({
              ...sig,
              createdAt: new Date(sig.createdAt),
              lastUsed: sig.lastUsed ? new Date(sig.lastUsed) : new Date(),
              deviceInfo: sig.deviceInfo || this.getDefaultDeviceInfo()
            }));
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading signatures:', error);
          this.snackBar.open('Failed to load digital signatures', 'Close', { duration: 3000 });
          this.isLoading = false;
        }
      });
  }

  loadDeviceInfo(): void {
    this.apiService.getDeviceInfo()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<DeviceInfo>) => {
          if (response.success && response.data) {
            this.deviceInfo = {
              ...response.data,
              lastConnected: new Date(response.data.lastConnected || Date.now())
            };
          }
        },
        error: (error) => {
          console.error('Error loading device info:', error);
          this.deviceInfo = this.getDefaultDeviceInfo();
        }
      });
  }

  loadSignatureStats(): void {
    this.apiService.getSignatureStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<SignatureStats>) => {
          if (response.success && response.data) {
            this.signatureStats = {
              ...response.data,
              lastSignatureDate: response.data.lastSignatureDate ? new Date(response.data.lastSignatureDate) : undefined
            };
          }
        },
        error: (error) => {
          console.error('Error loading signature stats:', error);
        }
      });
  }

  refreshData(): void {
    this.loadSignatures();
    this.loadDeviceInfo();
    this.loadSignatureStats();
  }

  testDeviceConnection(): void {
    this.isTestingConnection = true;
    this.apiService.testDeviceConnection()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            this.snackBar.open('Device connection test successful', 'Close', { duration: 3000 });
            this.loadDeviceInfo();
          } else {
            this.snackBar.open('Device connection test failed', 'Close', { duration: 3000 });
          }
          this.isTestingConnection = false;
        },
        error: (error) => {
          console.error('Error testing device connection:', error);
          this.snackBar.open('Device connection test failed', 'Close', { duration: 3000 });
          this.isTestingConnection = false;
        }
      });
  }

  generateSignature(): void {
    if (this.generateForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    this.isGenerating = true;
    const formData = this.generateForm.value;

    this.apiService.generateSignatureWithDevice(formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<DigitalSignature>) => {
          if (response.success && response.data) {
            this.snackBar.open('Digital signature generated successfully', 'Close', { duration: 3000 });
            this.generateForm.reset();
            this.showGenerateForm = false;
            this.loadSignatures();
            this.loadSignatureStats();
          } else {
            this.snackBar.open('Failed to generate signature', 'Close', { duration: 3000 });
          }
          this.isGenerating = false;
        },
        error: (error) => {
          console.error('Error generating signature:', error);
          this.snackBar.open('Error generating signature', 'Close', { duration: 3000 });
          this.isGenerating = false;
        }
      });
  }

  toggleSignatureStatus(signature: DigitalSignature): void {
    const action = signature.isActive ? 'deactivate' : 'activate';
    const apiCall = signature.isActive ? 
      this.apiService.deactivateSignature(signature.id) :
      this.apiService.renewSignature(signature.id);

    apiCall.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            signature.isActive = !signature.isActive;
            this.snackBar.open(`Signature ${action}d successfully`, 'Close', { duration: 3000 });
            this.loadSignatureStats();
          } else {
            this.snackBar.open(`Failed to ${action} signature`, 'Close', { duration: 3000 });
          }
        },
        error: (error) => {
          console.error(`Error ${action}ing signature:`, error);
          this.snackBar.open(`Error ${action}ing signature`, 'Close', { duration: 3000 });
        }
      });
  }

  deleteSignature(signature: DigitalSignature): void {
    if (confirm(`Are you sure you want to delete the signature for ${signature.authorityName}?`)) {
      this.apiService.deleteSignature(signature.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response: ApiResponse<any>) => {
            if (response.success) {
              this.snackBar.open('Signature deleted successfully', 'Close', { duration: 3000 });
              this.loadSignatures();
              this.loadSignatureStats();
            } else {
              this.snackBar.open('Failed to delete signature', 'Close', { duration: 3000 });
            }
          },
          error: (error) => {
            console.error('Error deleting signature:', error);
            this.snackBar.open('Error deleting signature', 'Close', { duration: 3000 });
          }
        });
    }
  }

  downloadSignature(signature: DigitalSignature): void {
    this.apiService.downloadSignature(signature.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success && response.data) {
            // Create download link
            const blob = new Blob([response.data], { type: 'application/octet-stream' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `signature_${signature.authorityName.replace(/\s+/g, '_')}.png`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
            
            this.snackBar.open('Signature downloaded successfully', 'Close', { duration: 3000 });
          }
        },
        error: (error) => {
          console.error('Error downloading signature:', error);
          this.snackBar.open('Error downloading signature', 'Close', { duration: 3000 });
        }
      });
  }

  validateSignature(signature: DigitalSignature): void {
    this.apiService.validateSignature(signature)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<boolean>) => {
          if (response.success) {
            const isValid = response.data;
            this.snackBar.open(
              `Signature validation: ${isValid ? 'Valid' : 'Invalid'}`, 
              'Close', 
              { duration: 3000 }
            );
          }
        },
        error: (error) => {
          console.error('Error validating signature:', error);
          this.snackBar.open('Error validating signature', 'Close', { duration: 3000 });
        }
      });
  }

  getSignatureImage(signature: DigitalSignature): string {
    if (signature.signatureImage) {
      return `data:image/png;base64,${signature.signatureImage}`;
    }
    return 'assets/images/signature-placeholder.png';
  }

  getStatusColor(status: boolean): string {
    return status ? 'primary' : 'warn';
  }

  getStatusIcon(status: boolean): string {
    return status ? 'check_circle' : 'cancel';
  }

  getDeviceStatusColor(status: boolean): string {
    return status ? 'primary' : 'warn';
  }

  getDeviceStatusIcon(status: boolean): string {
    return status ? 'wifi' : 'wifi_off';
  }

  private getDefaultDeviceInfo(): DeviceInfo {
    return {
      isConnected: false,
      deviceName: 'PROXKey Device',
      serialNumber: 'Unknown',
      firmwareVersion: 'Unknown',
      availableSignatures: 0,
      lastConnected: new Date()
    };
  }

  // Table sorting and pagination
  ngAfterViewInit() {
    // This would be implemented if using MatTableDataSource
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    // Implement filtering logic
  }
}
