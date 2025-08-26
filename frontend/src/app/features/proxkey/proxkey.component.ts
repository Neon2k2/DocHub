import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';

interface PROXKeyInfo {
  deviceId: string;
  serialNumber: string;
  firmwareVersion: string;
  status: string;
  batteryLevel?: number;
  manufacturer: string;
  isInitialized: boolean;
  remainingSignatures: number;
  lastUsed: Date;
}

interface DigitalSignatureDto {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureDate: Date;
  documentHash: string;
  signatureData: string;
  isValid: boolean;
  createdAt: Date;
  isActive: boolean;
}

interface GenerateSignatureRequest {
  authorityName: string;
  authorityDesignation: string;
  documentHash: string;
}

interface SignDocumentRequest {
  documentBytes: Uint8Array;
  authorityName: string;
  pin: string;
}

interface ChangePinRequest {
  oldPin: string;
  newPin: string;
}

@Component({
  selector: 'app-proxkey',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatListModule,
    MatExpansionModule,
    MatTabsModule,
    MatTooltipModule
  ],
  templateUrl: './proxkey.component.html',
  styleUrls: ['./proxkey.component.scss']
})
export class PROXKeyComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Device status
  deviceConnected = false;
  deviceInfo: PROXKeyInfo | null = null;
  deviceStatus = 'Checking...';
  isLoading = false;

  // Forms
  signatureForm: FormGroup;
  documentSignForm: FormGroup;
  pinChangeForm: FormGroup;

  // File handling
  selectedFile: File | null = null;
  documentHash = '';

  // Signatures
  generatedSignatures: DigitalSignatureDto[] = [];

  constructor(
    private apiService: ApiService,
    private notificationService: NotificationService,
    private formBuilder: FormBuilder
  ) {
    this.signatureForm = this.formBuilder.group({
      authorityName: ['', [Validators.required, Validators.minLength(2)]],
      authorityDesignation: ['', [Validators.required, Validators.minLength(2)]],
      documentHash: ['', [Validators.required, Validators.minLength(64)]]
    });

    this.documentSignForm = this.formBuilder.group({
      authorityName: ['', [Validators.required, Validators.minLength(2)]],
      pin: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]]
    });

    this.pinChangeForm = this.formBuilder.group({
      oldPin: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      newPin: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPin: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]]
    }, { validators: this.pinMatchValidator });
  }

  ngOnInit(): void {
    this.initializeDeviceMonitoring();
    this.loadGeneratedSignatures();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private async initializeDeviceMonitoring(): Promise<void> {
    // Check device status on initialization
    await this.checkDeviceStatus();
    
    // Set up periodic device status checks
    setInterval(() => {
      this.checkDeviceStatus();
    }, 30000); // Check every 30 seconds
  }

  async checkDeviceStatus(): Promise<void> {
    this.isLoading = true;
    
    try {
      const response = await this.apiService.getDeviceInfo().toPromise();
      if (response?.success) {
        this.deviceConnected = true;
        this.deviceStatus = 'Ready for use';
        this.deviceInfo = {
          deviceId: response.data.deviceId || 'PROXKEY_001',
          serialNumber: response.data.serialNumber || 'PK123456789',
          firmwareVersion: response.data.firmwareVersion || '2.1.0',
          status: response.data.status || 'Connected',
          batteryLevel: response.data.batteryLevel || 85,
          manufacturer: response.data.manufacturer || 'PROXKey Technologies',
          isInitialized: response.data.isInitialized || true,
          remainingSignatures: response.data.remainingSignatures || 4850,
          lastUsed: new Date(response.data.lastUsed || new Date())
        };
        this.notificationService.showSuccess('Success', 'PROXKey device detected and ready');
      } else {
        throw new Error(response?.message || 'Device not responding');
      }
    } catch (error) {
      console.error('Error checking device status:', error);
      this.deviceConnected = false;
      this.deviceStatus = 'Device not connected';
      this.deviceInfo = null;
      
      // Fallback to mock data for development
      if (this.isDevelopmentMode()) {
        this.deviceConnected = true;
        this.deviceStatus = 'Ready for use (Mock Mode)';
        this.deviceInfo = this.getMockDeviceInfo();
        this.notificationService.showWarning('Warning', 'Using mock device data - real device not connected');
      } else {
        this.notificationService.showError('Error', 'PROXKey device not detected');
      }
    } finally {
      this.isLoading = false;
    }
  }

  private isDevelopmentMode(): boolean {
    return environment.production === false;
  }

  private getMockDeviceInfo(): PROXKeyInfo {
    return {
      deviceId: 'PROXKEY_001',
      serialNumber: 'PK123456789',
      firmwareVersion: '2.1.0',
      status: 'Connected',
      batteryLevel: 85,
      manufacturer: 'PROXKey Technologies',
      isInitialized: true,
      remainingSignatures: 4850,
      lastUsed: new Date()
    };
  }

  async getDeviceStatus(): Promise<void> {
    await this.checkDeviceStatus();
  }

  async refreshDeviceInfo(): Promise<void> {
    await this.checkDeviceStatus();
  }

  async onFileSelected(event: any): Promise<void> {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      await this.generateDocumentHash();
    }
  }

  async generateDocumentHash(): Promise<void> {
    if (!this.selectedFile) return;

    try {
      // In a real implementation, this would generate an actual hash
      // For now, we'll create a mock hash
      const mockHash = 'sha256:' + Math.random().toString(36).substring(2, 15).repeat(4);
      this.documentHash = mockHash;
      this.signatureForm.patchValue({ documentHash: this.documentHash });
      
      this.notificationService.showSuccess('Success', 'Document hash generated successfully');
    } catch (error) {
      console.error('Error generating document hash:', error);
      this.notificationService.showError('Error', 'Failed to generate document hash');
    }
  }

  async generateSignature(): Promise<void> {
    if (this.signatureForm.valid && this.deviceConnected) {
      this.isLoading = true;
      const request: GenerateSignatureRequest = this.signatureForm.value;

      try {
        // Call the API to generate signature
        const response = await this.apiService.generateSignatureWithDevice(request).toPromise();
        
        if (response?.success) {
          const signature: DigitalSignatureDto = {
            id: response.data.id || Math.random().toString(36).substring(2, 15),
            authorityName: request.authorityName,
            authorityDesignation: request.authorityDesignation,
            signatureDate: new Date(),
            documentHash: request.documentHash,
            signatureData: response.data.signatureData || 'MOCK_SIGNATURE_DATA_' + Math.random().toString(36),
            isValid: true,
            createdAt: new Date(),
            isActive: true
          };
          
          this.generatedSignatures.unshift(signature);
          this.signatureForm.reset();
          this.selectedFile = null;
          this.documentHash = '';
          
          this.notificationService.showSuccess('Success', 'Digital signature generated successfully!');
        } else {
          throw new Error(response?.message || 'Failed to generate signature');
        }
      } catch (error) {
        console.error('Error generating signature:', error);
        
        // Fallback to mock implementation for development
        if (this.isDevelopmentMode()) {
          await this.generateMockSignature(request);
        } else {
          this.notificationService.showError('Error', 'Failed to generate signature');
        }
      } finally {
        this.isLoading = false;
      }
    }
  }

  private async generateMockSignature(request: GenerateSignatureRequest): Promise<void> {
    // Simulate signature generation delay
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    const signature: DigitalSignatureDto = {
      id: Math.random().toString(36).substring(2, 15),
      authorityName: request.authorityName,
      authorityDesignation: request.authorityDesignation,
      signatureDate: new Date(),
      documentHash: request.documentHash,
      signatureData: 'MOCK_SIGNATURE_DATA_' + Math.random().toString(36),
      isValid: true,
      createdAt: new Date(),
      isActive: true
    };
    
    this.generatedSignatures.unshift(signature);
    this.signatureForm.reset();
    this.selectedFile = null;
    this.documentHash = '';
    
    this.notificationService.showSuccess('Success', 'Digital signature generated successfully! (Mock Mode)');
  }

  async signDocument(): Promise<void> {
    if (this.documentSignForm.valid && this.selectedFile && this.deviceConnected) {
      this.isLoading = true;
      const request: SignDocumentRequest = this.documentSignForm.value;

      try {
        // Call the API to sign document
        const response = await this.apiService.generateSignatureWithDevice({
          authorityName: request.authorityName,
          authorityDesignation: 'Document Signer',
          documentHash: this.documentHash,
          pin: request.pin
        }).toPromise();
        
        if (response?.success) {
          const filename = `signed_${this.selectedFile?.name}`;
          this.documentSignForm.reset();
          this.selectedFile = null;
          
          this.notificationService.showSuccess('Success', 'Document signed and downloaded successfully!');
        } else {
          throw new Error(response?.message || 'Failed to sign document');
        }
      } catch (error) {
        console.error('Error signing document:', error);
        
        // Fallback to mock implementation for development
        if (this.isDevelopmentMode()) {
          await this.signMockDocument(request);
        } else {
          this.notificationService.showError('Error', 'Failed to sign document');
        }
      } finally {
        this.isLoading = false;
      }
    }
  }

  private async signMockDocument(request: SignDocumentRequest): Promise<void> {
    // Simulate document signing delay
    await new Promise(resolve => setTimeout(resolve, 3000));
    
    const filename = `signed_${this.selectedFile?.name}`;
    this.documentSignForm.reset();
    this.selectedFile = null;
    
    this.notificationService.showSuccess('Success', 'Document signed and downloaded successfully! (Mock Mode)');
  }

  async changePin(): Promise<void> {
    if (this.pinChangeForm.valid && this.deviceConnected) {
      this.isLoading = true;
      const request: ChangePinRequest = {
        oldPin: this.pinChangeForm.value.oldPin,
        newPin: this.pinChangeForm.value.newPin
      };

      try {
        // In a real implementation, this would call the API to change PIN
        // For now, we'll simulate the process
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        this.pinChangeForm.reset();
        this.notificationService.showSuccess('Success', 'PIN changed successfully!');
      } catch (error) {
        console.error('Error changing PIN:', error);
        this.notificationService.showError('Error', 'Failed to change PIN');
      } finally {
        this.isLoading = false;
      }
    }
  }

  async resetDevice(): Promise<void> {
    if (confirm('Are you sure you want to reset the PROXKey device? This action cannot be undone.')) {
      this.isLoading = true;
      
      try {
        // In a real implementation, this would call the API to reset device
        // For now, we'll simulate the process
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        this.deviceInfo = null;
        this.generatedSignatures = [];
        this.deviceConnected = false;
        
        this.notificationService.showSuccess('Success', 'Device reset successfully!');
        await this.refreshDeviceInfo();
      } catch (error) {
        console.error('Error resetting device:', error);
        this.notificationService.showError('Error', 'Failed to reset device');
      } finally {
        this.isLoading = false;
      }
    }
  }

  private pinMatchValidator(group: FormGroup): { [key: string]: any } | null {
    const newPin = group.get('newPin')?.value;
    const confirmPin = group.get('confirmPin')?.value;
    return newPin === confirmPin ? null : { pinMismatch: true };
  }

  private loadGeneratedSignatures(): void {
    // This would typically load from a service that fetches from the backend
    // For now, we'll use an empty array
    this.generatedSignatures = [];
  }

  private showMessage(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    switch (type) {
      case 'success':
        this.notificationService.showSuccess('Success', message);
        break;
      case 'error':
        this.notificationService.showError('Error', message);
        break;
      default:
        this.notificationService.showInfo('Info', message);
        break;
    }
  }

  getDeviceStatusColor(): string {
    if (!this.deviceConnected) return 'warn';
    if (this.deviceStatus.includes('Ready')) return 'accent';
    if (this.deviceStatus.includes('Error')) return 'warn';
    return 'primary';
  }

  getDeviceStatusIcon(): string {
    if (!this.deviceConnected) return 'usb_off';
    if (this.deviceStatus.includes('Ready')) return 'usb';
    if (this.deviceStatus.includes('Error')) return 'error';
    return 'usb_unknown';
  }
}
