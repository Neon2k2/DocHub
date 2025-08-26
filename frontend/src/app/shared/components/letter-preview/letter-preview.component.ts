import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';

import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';

export interface LetterPreviewData {
  id: string;
  templateId: string;
  templateName: string;
  employeeId: string;
  employeeName: string;
  employeeEmail: string;
  employeeDepartment: string;
  letterContent: string;
  placeholders: { [key: string]: string };
  signatureId?: string;
  signatureData?: SignatureData;
  letterDate: Date;
  status: 'draft' | 'preview' | 'generated' | 'sent';
  attachments?: AttachmentData[];
}

export interface SignatureData {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureImage: string;
  signatureDate: Date;
  isDigital: boolean;
  certificateInfo?: CertificateInfo;
}

export interface CertificateInfo {
  issuer: string;
  validFrom: Date;
  validTo: Date;
  serialNumber: string;
}

export interface AttachmentData {
  id: string;
  name: string;
  type: string;
  size: number;
  url: string;
}

export interface LetterPreviewConfig {
  showSignature?: boolean;
  showAttachments?: boolean;
  showActions?: boolean;
  allowEdit?: boolean;
  showWatermark?: boolean;
  previewMode?: 'draft' | 'final' | 'print';
}

@Component({
  selector: 'app-letter-preview',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatDividerModule,
    MatListModule,
    MatTabsModule,
    MatTooltipModule,
    MatDialogModule,
    MatMenuModule,
    MatSnackBarModule
  ],
  template: `
    <div class="letter-preview-container" [class.preview-mode]="config.previewMode === 'final'">
      
      <!-- Preview Header -->
      <div class="preview-header">
        <div class="header-content">
          <h3 class="header-title">
            <mat-icon class="title-icon">preview</mat-icon>
            Letter Preview
          </h3>
          <p class="header-description">
            Preview your letter before generation and sending
          </p>
        </div>
        <div class="header-actions">
          <button mat-stroked-button (click)="togglePreviewMode()" class="mode-toggle-btn">
            <mat-icon>{{ config.previewMode === 'final' ? 'edit' : 'visibility' }}</mat-icon>
            {{ config.previewMode === 'final' ? 'Edit Mode' : 'Preview Mode' }}
          </button>
          <button mat-stroked-button (click)="printLetter()" class="print-btn">
            <mat-icon>print</mat-icon>
            Print
          </button>
          <button mat-stroked-button (click)="downloadPDF()" class="download-btn">
            <mat-icon>download</mat-icon>
            Download PDF
          </button>
        </div>
      </div>

      <!-- Letter Content Preview -->
      <div class="letter-content-preview">
        <div class="letter-paper" [class.final-mode]="config.previewMode === 'final'">
          
          <!-- Company Header -->
          <div class="company-header">
            <div class="company-logo">
              <mat-icon class="logo-icon">business</mat-icon>
            </div>
            <div class="company-info">
              <h1 class="company-name">DocHub Corporation</h1>
              <p class="company-address">123 Business Street, Tech City, TC 12345</p>
              <p class="company-contact">Phone: +1 (555) 123-4567 | Email: info@dochub.com</p>
            </div>
          </div>

          <!-- Letter Date -->
          <div class="letter-date">
            <p>{{ letterData.letterDate | date:'longDate' }}</p>
          </div>

          <!-- Employee Information -->
          <div class="employee-info">
            <p><strong>To:</strong> {{ letterData.employeeName }}</p>
            <p><strong>Employee ID:</strong> {{ letterData.employeeId }}</p>
            <p><strong>Department:</strong> {{ letterData.employeeDepartment }}</p>
            <p><strong>Email:</strong> {{ letterData.employeeEmail }}</p>
          </div>

          <!-- Letter Subject -->
          <div class="letter-subject">
            <h2>{{ letterData.templateName }}</h2>
          </div>

          <!-- Letter Body -->
          <div class="letter-body" [innerHTML]="processedLetterContent"></div>

          <!-- Signature Section -->
          <div class="signature-section" *ngIf="config.showSignature && letterData.signatureData">
            <div class="signature-content">
              <div class="signature-image-container">
                <img 
                  [src]="letterData.signatureData.signatureImage" 
                  [alt]="'Digital Signature of ' + letterData.signatureData.authorityName"
                  class="signature-image"
                  (error)="onSignatureImageError($event)">
                <div class="signature-watermark" *ngIf="config.showWatermark">
                  <mat-icon>verified</mat-icon>
                  <span>Digitally Signed</span>
                </div>
              </div>
              <div class="signature-details">
                <p class="authority-name">{{ letterData.signatureData.authorityName }}</p>
                <p class="authority-designation">{{ letterData.signatureData.authorityDesignation }}</p>
                <p class="signature-date">{{ letterData.signatureData.signatureDate | date:'medium' }}</p>
                <div class="certificate-info" *ngIf="letterData.signatureData.certificateInfo">
                  <p class="cert-issuer">Issued by: {{ letterData.signatureData.certificateInfo.issuer }}</p>
                  <p class="cert-validity">Valid: {{ letterData.signatureData.certificateInfo.validFrom | date:'shortDate' }} - {{ letterData.signatureData.certificateInfo.validTo | date:'shortDate' }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Attachments Section -->
          <div class="attachments-section" *ngIf="config.showAttachments && letterData.attachments && letterData.attachments.length > 0">
            <div class="attachments-header">
              <h4>Attachments</h4>
            </div>
            <div class="attachments-list">
              <div 
                *ngFor="let attachment of letterData.attachments" 
                class="attachment-item">
                <mat-icon class="attachment-icon">{{ getFileIcon(attachment.type) }}</mat-icon>
                <div class="attachment-info">
                  <span class="attachment-name">{{ attachment.name }}</span>
                  <span class="attachment-size">{{ formatFileSize(attachment.size) }}</span>
                </div>
                <button mat-icon-button (click)="downloadAttachment(attachment)" class="download-attachment-btn">
                  <mat-icon>download</mat-icon>
                </button>
              </div>
            </div>
          </div>

          <!-- Watermark -->
          <div class="watermark" *ngIf="config.showWatermark && config.previewMode === 'draft'">
            <span>DRAFT</span>
          </div>
        </div>
      </div>

      <!-- Preview Actions -->
      <div class="preview-actions" *ngIf="config.showActions">
        <div class="action-buttons">
          <button 
            mat-stroked-button 
            (click)="editLetter()"
            [disabled]="!config.allowEdit"
            class="edit-btn">
            <mat-icon>edit</mat-icon>
            Edit Letter
          </button>
          
          <button 
            mat-raised-button 
            color="primary"
            (click)="generateLetter()"
            [disabled]="isGenerating"
            class="generate-btn">
            <mat-icon *ngIf="!isGenerating">create</mat-icon>
            <mat-spinner *ngIf="isGenerating" diameter="20"></mat-spinner>
            {{ isGenerating ? 'Generating...' : 'Generate Letter' }}
          </button>
          
          <button 
            mat-raised-button 
            color="accent"
            (click)="sendLetter()"
            [disabled]="!canSendLetter()"
            class="send-btn">
            <mat-icon>send</mat-icon>
            Send Letter
          </button>
        </div>
      </div>

      <!-- Letter Properties Panel -->
      <div class="letter-properties">
        <mat-expansion-panel class="properties-panel">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <mat-icon>settings</mat-icon>
              Letter Properties
            </mat-panel-title>
          </mat-expansion-panel-header>
          
          <div class="properties-content">
            <div class="property-grid">
              <div class="property-item">
                <label>Template:</label>
                <span>{{ letterData.templateName }}</span>
              </div>
              <div class="property-item">
                <label>Employee:</label>
                <span>{{ letterData.employeeName }}</span>
              </div>
              <div class="property-item">
                <label>Department:</label>
                <span>{{ letterData.employeeDepartment }}</span>
              </div>
              <div class="property-item">
                <label>Date:</label>
                <span>{{ letterData.letterDate | date:'medium' }}</span>
              </div>
              <div class="property-item">
                <label>Status:</label>
                <span class="status-badge" [class]="letterData.status">{{ letterData.status | titlecase }}</span>
              </div>
              <div class="property-item">
                <label>Signature:</label>
                <span>{{ letterData.signatureData ? 'Applied' : 'Not Applied' }}</span>
              </div>
            </div>
            
            <div class="placeholders-section" *ngIf="Object.keys(letterData.placeholders).length > 0">
              <h5>Dynamic Fields</h5>
              <div class="placeholders-list">
                <div 
                  *ngFor="let placeholder of getPlaceholderEntries()" 
                  class="placeholder-item">
                  <span class="placeholder-key">{{ placeholder.key }}</span>
                  <span class="placeholder-value">{{ placeholder.value }}</span>
                </div>
              </div>
            </div>
          </div>
        </mat-expansion-panel>
      </div>
    </div>
  `,
  styleUrls: ['./letter-preview.component.scss']
})
export class LetterPreviewComponent implements OnInit, OnDestroy {
  @Input() letterData!: LetterPreviewData;
  @Input() config: LetterPreviewConfig = {
    showSignature: true,
    showAttachments: true,
    showActions: true,
    allowEdit: true,
    showWatermark: true,
    previewMode: 'draft'
  };
  
  @Output() letterEdited = new EventEmitter<LetterPreviewData>();
  @Output() letterGenerated = new EventEmitter<string>();
  @Output() letterSent = new EventEmitter<string>();
  @Output() previewModeChanged = new EventEmitter<string>();

  @ViewChild('letterContent', { static: false }) letterContent!: ElementRef;

  processedLetterContent = '';
  isGenerating = false;
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private apiService: ApiService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.processLetterContent();
  }

  ngOnDestroy(): void {
    this.destroy$?.next();
    this.destroy$?.complete();
  }

  private processLetterContent(): void {
    let content = this.letterData.letterContent;
    
    // Replace placeholders with actual values
    Object.keys(this.letterData.placeholders).forEach(key => {
      const placeholder = `{{${key}}}`;
      const value = this.letterData.placeholders[key];
      content = content.replace(new RegExp(placeholder, 'g'), value);
    });
    
    // Replace employee-specific placeholders
    content = content.replace(/\{\{employeeName\}\}/g, this.letterData.employeeName);
    content = content.replace(/\{\{employeeId\}\}/g, this.letterData.employeeId);
    content = content.replace(/\{\{employeeDepartment\}\}/g, this.letterData.employeeDepartment);
    content = content.replace(/\{\{employeeEmail\}\}/g, this.letterData.employeeEmail);
    content = content.replace(/\{\{currentDate\}\}/g, new Date().toLocaleDateString());
    
    this.processedLetterContent = content;
  }

  togglePreviewMode(): void {
    this.config.previewMode = this.config.previewMode === 'draft' ? 'final' : 'draft';
    this.previewModeChanged.emit(this.config.previewMode);
  }

  printLetter(): void {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      const letterContent = this.letterContent.nativeElement.innerHTML;
      printWindow.document.write(`
        <html>
          <head>
            <title>${this.letterData.templateName} - ${this.letterData.employeeName}</title>
            <style>
              body { font-family: Arial, sans-serif; margin: 20px; }
              .letter-paper { max-width: 800px; margin: 0 auto; }
              .company-header { text-align: center; margin-bottom: 30px; }
              .letter-date { text-align: right; margin-bottom: 20px; }
              .employee-info { margin-bottom: 20px; }
              .letter-subject { margin-bottom: 20px; }
              .letter-body { line-height: 1.6; margin-bottom: 30px; }
              .signature-section { margin-top: 50px; }
              .signature-image { max-width: 200px; height: auto; }
              @media print { .no-print { display: none; } }
            </style>
          </head>
          <body>
            ${letterContent}
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  }

  async downloadPDF(): Promise<void> {
    try {
      this.notificationService.showInfo('PDF Generation', 'Generating PDF...');
      
      // TODO: Implement actual PDF generation using Syncfusion
      // For now, simulate PDF generation
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      this.notificationService.showSuccess('PDF Generated', 'Letter PDF has been generated successfully');
      
      // TODO: Trigger actual download
      // this.apiService.downloadLetterPDF(this.letterData.id).subscribe(...)
      
    } catch (error) {
      console.error('Error generating PDF:', error);
      this.notificationService.showError('PDF Generation Failed', 'Failed to generate PDF');
    }
  }

  editLetter(): void {
    if (!this.config.allowEdit) return;
    
    this.letterEdited.emit(this.letterData);
    this.notificationService.showInfo('Edit Mode', 'Letter editing mode activated');
  }

  async generateLetter(): Promise<void> {
    try {
      this.isGenerating = true;
      this.notificationService.showInfo('Letter Generation', 'Generating letter...');
      
      // TODO: Implement actual letter generation using Syncfusion
      // For now, simulate generation process
      await new Promise(resolve => setTimeout(resolve, 3000));
      
      this.letterData.status = 'generated';
      this.letterGenerated.emit(this.letterData.id);
      
      this.notificationService.showSuccess('Letter Generated', 'Letter has been generated successfully');
      
    } catch (error) {
      console.error('Error generating letter:', error);
      this.notificationService.showError('Generation Failed', 'Failed to generate letter');
    } finally {
      this.isGenerating = false;
    }
  }

  async sendLetter(): Promise<void> {
    if (!this.canSendLetter()) return;
    
    try {
      this.notificationService.showInfo('Sending Letter', 'Preparing to send letter...');
      
      // TODO: Implement actual email sending using SendGrid
      // For now, simulate sending process
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      this.letterData.status = 'sent';
      this.letterSent.emit(this.letterData.id);
      
      this.notificationService.showSuccess('Letter Sent', 'Letter has been sent successfully');
      
    } catch (error) {
      console.error('Error sending letter:', error);
      this.notificationService.showError('Send Failed', 'Failed to send letter');
    }
  }

  canSendLetter(): boolean {
    return this.letterData.status === 'generated' && 
           !!this.letterData.signatureData &&
           !this.isGenerating;
  }

  downloadAttachment(attachment: AttachmentData): void {
    // TODO: Implement actual attachment download
    this.notificationService.showInfo('Download Started', `Downloading ${attachment.name}`);
    
    // Simulate download
    const link = document.createElement('a');
    link.href = attachment.url;
    link.download = attachment.name;
    link.click();
  }

  onSignatureImageError(event: any): void {
    console.error('Signature image failed to load:', event);
    // Replace with placeholder or default signature
    event.target.src = '/assets/images/default-signature.png';
  }

  getFileIcon(fileType: string): string {
    const iconMap: { [key: string]: string } = {
      'pdf': 'picture_as_pdf',
      'doc': 'description',
      'docx': 'description',
      'xls': 'table_chart',
      'xlsx': 'table_chart',
      'txt': 'article',
      'jpg': 'image',
      'jpeg': 'image',
      'png': 'image'
    };
    
    const extension = fileType.toLowerCase();
    return iconMap[extension] || 'attach_file';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getPlaceholderEntries(): { key: string; value: string }[] {
    return Object.entries(this.letterData.placeholders).map(([key, value]) => ({
      key,
      value
    }));
  }
}
