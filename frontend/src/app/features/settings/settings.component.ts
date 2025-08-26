import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { Subject, takeUntil } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';

interface AppSettings {
  general: {
    companyName: string;
    companyLogo: string;
    defaultLanguage: string;
    timezone: string;
    dateFormat: string;
    currency: string;
  };
  email: {
    smtpServer: string;
    smtpPort: number;
    smtpUsername: string;
    smtpPassword: string;
    fromEmail: string;
    fromName: string;
    enableNotifications: boolean;
  };
  document: {
    defaultTemplatePath: string;
    outputDirectory: string;
    enableVersioning: boolean;
    maxFileSize: number;
    allowedFileTypes: string[];
  };
  security: {
    sessionTimeout: number;
    maxLoginAttempts: number;
    enableTwoFactor: boolean;
    passwordPolicy: string;
    enableAuditLog: boolean;
  };
  integration: {
    sendGridApiKey: string;
    syncfusionLicense: string;
    proxKeyEndpoint: string;
    enableWebhooks: boolean;
  };
}

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatDividerModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatCardModule
  ]
})
export class SettingsComponent implements OnInit, OnDestroy {
  settingsForm: FormGroup;
  loading = false;
  saving = false;
  currentSettings: AppSettings | null = null;
  hasChanges = false;

  languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Spanish' },
    { code: 'fr', name: 'French' },
    { code: 'de', name: 'German' },
    { code: 'it', name: 'Italian' },
    { code: 'pt', name: 'Portuguese' },
    { code: 'ru', name: 'Russian' },
    { code: 'zh', name: 'Chinese' },
    { code: 'ja', name: 'Japanese' },
    { code: 'ko', name: 'Korean' }
  ];

  timezones = [
    { value: 'UTC', label: 'UTC (Coordinated Universal Time)' },
    { value: 'America/New_York', label: 'Eastern Time (ET)' },
    { value: 'America/Chicago', label: 'Central Time (CT)' },
    { value: 'America/Denver', label: 'Mountain Time (MT)' },
    { value: 'America/Los_Angeles', label: 'Pacific Time (PT)' },
    { value: 'Europe/London', label: 'London (GMT)' },
    { value: 'Europe/Paris', label: 'Paris (CET)' },
    { value: 'Asia/Tokyo', label: 'Tokyo (JST)' },
    { value: 'Asia/Shanghai', label: 'Shanghai (CST)' },
    { value: 'Australia/Sydney', label: 'Sydney (AEDT)' }
  ];

  currencies = [
    { code: 'USD', name: 'US Dollar ($)' },
    { code: 'EUR', name: 'Euro (€)' },
    { code: 'GBP', name: 'British Pound (£)' },
    { code: 'JPY', name: 'Japanese Yen (¥)' },
    { code: 'CAD', name: 'Canadian Dollar (C$)' },
    { code: 'AUD', name: 'Australian Dollar (A$)' },
    { code: 'CHF', name: 'Swiss Franc (CHF)' },
    { code: 'CNY', name: 'Chinese Yuan (¥)' },
    { code: 'INR', name: 'Indian Rupee (₹)' },
    { code: 'BRL', name: 'Brazilian Real (R$)' }
  ];

  dateFormats = [
    { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY (US)' },
    { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY (EU)' },
    { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD (ISO)' },
    { value: 'DD-MM-YYYY', label: 'DD-MM-YYYY (EU)' },
    { value: 'MM-DD-YYYY', label: 'MM-DD-YYYY (US)' }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private notificationService: NotificationService,
    private themeService: ThemeService,
    private snackBar: MatSnackBar
  ) {
    this.settingsForm = this.createSettingsForm();
  }

  ngOnInit(): void {
    this.loadSettings();
    this.setupFormChangeDetection();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createSettingsForm(): FormGroup {
    return this.fb.group({
      general: this.fb.group({
        companyName: ['', Validators.required],
        companyLogo: [''],
        defaultLanguage: ['en', Validators.required],
        timezone: ['UTC', Validators.required],
        dateFormat: ['MM/DD/YYYY', Validators.required],
        currency: ['USD', Validators.required]
      }),
      email: this.fb.group({
        smtpServer: ['', Validators.required],
        smtpPort: [587, [Validators.required, Validators.min(1), Validators.max(65535)]],
        smtpUsername: ['', Validators.required],
        smtpPassword: ['', Validators.required],
        fromEmail: ['', [Validators.required, Validators.email]],
        fromName: ['', Validators.required],
        enableNotifications: [true]
      }),
      document: this.fb.group({
        defaultTemplatePath: ['', Validators.required],
        outputDirectory: ['', Validators.required],
        enableVersioning: [true],
        maxFileSize: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
        allowedFileTypes: [['docx', 'pdf', 'xlsx'], Validators.required]
      }),
      security: this.fb.group({
        sessionTimeout: [30, [Validators.required, Validators.min(5), Validators.max(480)]],
        maxLoginAttempts: [5, [Validators.required, Validators.min(3), Validators.max(10)]],
        enableTwoFactor: [false],
        passwordPolicy: ['medium', Validators.required],
        enableAuditLog: [true]
      }),
      integration: this.fb.group({
        sendGridApiKey: ['', Validators.required],
        syncfusionLicense: [''],
        proxKeyEndpoint: ['', Validators.required],
        enableWebhooks: [false]
      })
    });
  }

  private setupFormChangeDetection(): void {
    this.settingsForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.hasChanges = true;
      });
  }

  async loadSettings(): Promise<void> {
    this.loading = true;
    
    try {
      const response = await this.apiService.getApplicationSettings().toPromise();
      
      if (response?.success && response.data) {
        this.currentSettings = response.data;
        if (this.currentSettings) {
          this.settingsForm.patchValue(this.currentSettings);
        }
        this.hasChanges = false;
        this.notificationService.showSuccess('Success', 'Settings loaded successfully');
      } else {
        throw new Error(response?.message || 'Failed to load settings');
      }
    } catch (error) {
      console.error('Error loading settings:', error);
      
      // Fallback to default settings
      this.currentSettings = this.getDefaultSettings();
      if (this.currentSettings) {
        this.settingsForm.patchValue(this.currentSettings);
      }
      this.notificationService.showWarning('Warning', 'Using default settings - API connection failed');
    } finally {
      this.loading = false;
    }
  }

  private getDefaultSettings(): AppSettings {
    return {
      general: {
        companyName: 'DocHub Corporation',
        companyLogo: '',
        defaultLanguage: 'en',
        timezone: 'UTC',
        dateFormat: 'MM/DD/YYYY',
        currency: 'USD'
      },
      email: {
        smtpServer: 'smtp.sendgrid.net',
        smtpPort: 587,
        smtpUsername: 'apikey',
        smtpPassword: '',
        fromEmail: 'noreply@dochub.com',
        fromName: 'DocHub System',
        enableNotifications: true
      },
      document: {
        defaultTemplatePath: '/templates',
        outputDirectory: '/output',
        enableVersioning: true,
        maxFileSize: 10,
        allowedFileTypes: ['docx', 'pdf', 'xlsx']
      },
      security: {
        sessionTimeout: 30,
        maxLoginAttempts: 5,
        enableTwoFactor: false,
        passwordPolicy: 'medium',
        enableAuditLog: true
      },
      integration: {
        sendGridApiKey: '',
        syncfusionLicense: '',
        proxKeyEndpoint: 'http://localhost:5000/api/proxkey',
        enableWebhooks: false
      }
    };
  }

  async saveSettings(): Promise<void> {
    if (this.settingsForm.valid) {
      this.saving = true;
      
      try {
        const settings = this.settingsForm.value;
        const response = await this.apiService.updateApplicationSettings(settings).toPromise();
        
        if (response?.success) {
          this.currentSettings = settings;
          this.hasChanges = false;
          this.notificationService.showSuccess('Success', 'Settings saved successfully');
          
          // Apply theme changes if needed
          if (settings.general.defaultLanguage !== this.currentSettings?.general.defaultLanguage) {
            // Reload the application to apply language changes
            window.location.reload();
          }
        } else {
          throw new Error(response?.message || 'Failed to save settings');
        }
      } catch (error) {
        console.error('Error saving settings:', error);
        this.notificationService.showError('Error', 'Failed to save settings');
      } finally {
        this.saving = false;
      }
    } else {
      this.markFormGroupTouched();
      this.notificationService.showWarning('Warning', 'Please fix validation errors before saving');
    }
  }

  async resetToDefaults(): Promise<void> {
    if (confirm('Are you sure you want to reset all settings to default values? This action cannot be undone.')) {
      this.loading = true;
      
      try {
        const response = await this.apiService.resetApplicationSettings().toPromise();
        
        if (response?.success) {
                this.currentSettings = this.getDefaultSettings();
      if (this.currentSettings) {
        this.settingsForm.patchValue(this.currentSettings);
      }
          this.hasChanges = false;
          this.notificationService.showSuccess('Success', 'Settings reset to defaults successfully');
        } else {
          throw new Error(response?.message || 'Failed to reset settings');
        }
      } catch (error) {
        console.error('Error resetting settings:', error);
        this.notificationService.showError('Error', 'Failed to reset settings');
      } finally {
        this.loading = false;
      }
    }
  }

  async exportSettings(): Promise<void> {
    try {
      // Create download link directly from current settings
      if (this.currentSettings) {
        const blob = new Blob([JSON.stringify(this.currentSettings, null, 2)], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `dochub_settings_${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
        
        this.notificationService.showSuccess('Success', 'Settings exported successfully');
      } else {
        throw new Error('No settings to export');
      }
    } catch (error) {
      console.error('Error exporting settings:', error);
      this.notificationService.showError('Error', 'Failed to export settings');
    }
  }

  async importSettings(): Promise<void> {
    // Create a file input element
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.json';
    fileInput.onchange = async (event: any) => {
      const file = event.target.files[0];
      if (file) {
        try {
          const content = await this.readFileAsText(file);
          const importedSettings = JSON.parse(content);
          
          // Validate imported settings
          if (this.validateImportedSettings(importedSettings)) {
            this.settingsForm.patchValue(importedSettings);
            this.hasChanges = true;
            this.notificationService.showSuccess('Success', 'Settings imported successfully');
          } else {
            this.notificationService.showError('Error', 'Invalid settings file format');
          }
        } catch (error) {
          console.error('Error importing settings:', error);
          this.notificationService.showError('Error', 'Failed to import settings file');
        }
      }
    };
    fileInput.click();
  }

  private readFileAsText(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target?.result as string);
      reader.onerror = (e) => reject(e);
      reader.readAsText(file);
    });
  }

  private validateImportedSettings(settings: any): boolean {
    // Basic validation - check if required sections exist
    return settings &&
           settings.general &&
           settings.email &&
           settings.document &&
           settings.security &&
           settings.integration;
  }

  async testEmailSettings(): Promise<void> {
    const emailSettings = this.settingsForm.get('email')?.value;
    
    if (emailSettings) {
      this.loading = true;
      
      try {
        const response = await this.apiService.testEmailSettings(emailSettings).toPromise();
        
        if (response?.success) {
          this.notificationService.showSuccess('Success', 'Email settings test successful! Check your inbox for the test email.');
        } else {
          throw new Error(response?.message || 'Email test failed');
        }
      } catch (error) {
        console.error('Error testing email settings:', error);
        this.notificationService.showError('Error', 'Email settings test failed. Please check your configuration.');
      } finally {
        this.loading = false;
      }
    }
  }

  async testProXKeyConnection(): Promise<void> {
    const proxKeyEndpoint = this.settingsForm.get('integration.prooxKeyEndpoint')?.value;
    
    if (proxKeyEndpoint) {
      this.loading = true;
      
      try {
        const response = await this.apiService.testProXKeyConnection(proxKeyEndpoint).toPromise();
        
        if (response?.success) {
          this.notificationService.showSuccess('Success', 'PROXKey connection test successful!');
        } else {
          throw new Error(response?.message || 'PROXKey connection test failed');
        }
      } catch (error) {
        console.error('Error testing PROXKey connection:', error);
        this.notificationService.showError('Error', 'PROXKey connection test failed. Please check your endpoint configuration.');
      } finally {
        this.loading = false;
      }
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.settingsForm.controls).forEach(key => {
      const control = this.settingsForm.get(key);
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach(nestedKey => {
          control.get(nestedKey)?.markAsTouched();
        });
      } else {
        control?.markAsTouched();
      }
    });
  }

  refreshSettings(): void {
    this.loadSettings();
  }

  onFileSelected(event: any): void {
    this.importSettings();
  }
}
