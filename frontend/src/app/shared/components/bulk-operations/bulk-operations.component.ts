import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
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
import { Subject } from 'rxjs';

import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';

export interface BulkOperationConfig {
  showProgress?: boolean;
  showOptions?: boolean;
  showValidation?: boolean;
  allowCustomization?: boolean;
  maxItems?: number;
  requireConfirmation?: boolean;
}

export interface BulkOperationItem {
  id: string;
  name: string;
  type: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  progress?: number;
  error?: string;
  metadata?: any;
}

export interface BulkOperationResult {
  totalItems: number;
  successfulItems: number;
  failedItems: number;
  results: BulkOperationItem[];
  summary: string;
}

@Component({
  selector: 'app-bulk-operations',
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
    MatDialogModule
  ],
  template: `
    <div class="bulk-operations" [class.processing]="isProcessing">
      
      <!-- Header Section -->
      <div class="bulk-header">
        <div class="header-content">
          <h3 class="header-title">
            <mat-icon class="title-icon">batch_prediction</mat-icon>
            Bulk Operations
          </h3>
          <p class="header-description">
            Process multiple items simultaneously for increased efficiency
          </p>
        </div>
        <div class="header-stats">
          <div class="stat-item">
            <span class="stat-value">{{ selectedItems.length }}</span>
            <span class="stat-label">Selected</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ config.maxItems || '∞' }}</span>
            <span class="stat-label">Max Items</span>
          </div>
        </div>
      </div>

      <!-- Operation Configuration -->
      <div class="operation-config" *ngIf="config.showOptions">
        <mat-expansion-panel class="config-panel">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <mat-icon>settings</mat-icon>
              Operation Configuration
            </mat-panel-title>
          </mat-expansion-panel-header>
          
          <form [formGroup]="operationForm" class="config-form">
            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Operation Type</mat-label>
                <mat-select formControlName="operationType">
                  <mat-option value="generate">Generate Letters</mat-option>
                  <mat-option value="send">Send Emails</mat-option>
                  <mat-option value="update">Update Status</mat-option>
                  <mat-option value="export">Export Data</mat-option>
                </mat-select>
              </mat-form-field>
              
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Priority</mat-label>
                <mat-select formControlName="priority">
                  <mat-option value="low">Low</mat-option>
                  <mat-option value="normal">Normal</mat-option>
                  <mat-option value="high">High</mat-option>
                  <mat-option value="urgent">Urgent</mat-option>
                </mat-select>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Batch Size</mat-label>
                <input matInput type="number" formControlName="batchSize" min="1" max="100">
                <mat-hint>Number of items to process in each batch</mat-hint>
              </mat-form-field>
              
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Delay Between Batches (ms)</mat-label>
                <input matInput type="number" formControlName="batchDelay" min="0" max="5000">
                <mat-hint>Delay to prevent overwhelming the system</mat-hint>
              </mat-form-field>
            </div>

            <div class="form-options">
              <mat-checkbox formControlName="continueOnError">
                Continue processing if individual items fail
              </mat-checkbox>
              <mat-checkbox formControlName="sendNotifications">
                Send email notifications upon completion
              </mat-checkbox>
              <mat-checkbox formControlName="saveToHistory">
                Save operation results to history
              </mat-checkbox>
            </div>
          </form>
        </mat-expansion-panel>
      </div>

      <!-- Selected Items Overview -->
      <div class="selected-items">
        <div class="items-header">
          <h4>Selected Items ({{ selectedItems.length }})</h4>
          <div class="items-actions">
            <button mat-stroked-button (click)="clearSelection()" class="clear-btn">
              <mat-icon>clear_all</mat-icon>
              Clear All
            </button>
            <button mat-stroked-button (click)="invertSelection()" class="invert-btn">
              <mat-icon>swap_horiz</mat-icon>
              Invert
            </button>
          </div>
        </div>

        <div class="items-grid">
          <div 
            *ngFor="let item of selectedItems" 
            class="item-card"
            [class.selected]="item.status === 'pending'"
            [class.processing]="item.status === 'processing'"
            [class.completed]="item.status === 'completed'"
            [class.failed]="item.status === 'failed'">
            
            <div class="item-header">
              <div class="item-status">
                <mat-icon *ngIf="item.status === 'pending'" class="status-icon pending">radio_button_unchecked</mat-icon>
                <mat-icon *ngIf="item.status === 'processing'" class="status-icon processing">sync</mat-icon>
                <mat-icon *ngIf="item.status === 'completed'" class="status-icon completed">check_circle</mat-icon>
                <mat-icon *ngIf="item.status === 'failed'" class="status-icon failed">error</mat-icon>
              </div>
              <div class="item-info">
                <div class="item-name">{{ item.name }}</div>
                <div class="item-type">{{ item.type }}</div>
              </div>
              <div class="item-actions">
                <button mat-icon-button (click)="removeItem(item.id)" class="remove-btn">
                  <mat-icon>close</mat-icon>
                </button>
              </div>
            </div>

            <div class="item-progress" *ngIf="item.status === 'processing' && config.showProgress">
              <mat-progress-bar 
                [value]="item.progress || 0" 
                mode="determinate"
                class="progress-bar">
              </mat-progress-bar>
              <span class="progress-text">{{ item.progress || 0 }}%</span>
            </div>

            <div class="item-error" *ngIf="item.status === 'failed' && item.error">
              <mat-icon class="error-icon">warning</mat-icon>
              <span class="error-text">{{ item.error }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Operation Actions -->
      <div class="operation-actions">
        <div class="action-buttons">
          <button 
            mat-raised-button 
            color="primary"
            [disabled]="!canStartOperation() || isProcessing"
            (click)="startOperation()"
            class="start-btn">
            <mat-icon *ngIf="!isProcessing">play_arrow</mat-icon>
            <mat-spinner *ngIf="isProcessing" diameter="20"></mat-spinner>
            {{ isProcessing ? 'Processing...' : 'Start Operation' }}
          </button>
          
          <button 
            mat-stroked-button 
            [disabled]="!isProcessing"
            (click)="pauseOperation()"
            class="pause-btn">
            <mat-icon>pause</mat-icon>
            Pause
          </button>
          
          <button 
            mat-stroked-button 
            [disabled]="!isProcessing"
            (click)="stopOperation()"
            class="stop-btn">
            <mat-icon>stop</mat-icon>
            Stop
          </button>
        </div>

        <div class="operation-status" *ngIf="isProcessing">
          <div class="status-info">
            <span class="status-label">Status:</span>
            <span class="status-value">{{ operationStatus }}</span>
          </div>
          <div class="overall-progress" *ngIf="config.showProgress">
            <mat-progress-bar 
              [value]="overallProgress" 
              mode="determinate"
              class="overall-progress-bar">
            </mat-progress-bar>
            <span class="progress-text">{{ overallProgress }}% Complete</span>
          </div>
        </div>
      </div>

      <!-- Results Summary -->
      <div class="results-summary" *ngIf="operationResult">
        <mat-expansion-panel class="results-panel">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <mat-icon>assessment</mat-icon>
              Operation Results
            </mat-panel-title>
          </mat-expansion-panel-header>
          
          <div class="results-content">
            <div class="results-stats">
              <div class="stat-card success">
                <mat-icon>check_circle</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ operationResult.successfulItems }}</span>
                  <span class="stat-label">Successful</span>
                </div>
              </div>
              <div class="stat-card failed">
                <mat-icon>error</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ operationResult.failedItems }}</span>
                  <span class="stat-label">Failed</span>
                </div>
              </div>
              <div class="stat-card total">
                <mat-icon>list</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ operationResult.totalItems }}</span>
                  <span class="stat-label">Total</span>
                </div>
              </div>
            </div>
            
            <div class="results-summary-text">
              <p>{{ operationResult.summary }}</p>
            </div>
            
            <div class="results-actions">
              <button mat-stroked-button (click)="exportResults()" class="export-btn">
                <mat-icon>download</mat-icon>
                Export Results
              </button>
              <button mat-stroked-button (click)="viewDetails()" class="details-btn">
                <mat-icon>visibility</mat-icon>
                View Details
              </button>
            </div>
          </div>
        </mat-expansion-panel>
      </div>
    </div>
  `,
  styleUrls: ['./bulk-operations.component.scss']
})
export class BulkOperationsComponent implements OnInit, OnDestroy {
  @Input() selectedItems: BulkOperationItem[] = [];
  @Input() config: BulkOperationConfig = {
    showProgress: true,
    showOptions: true,
    showValidation: true,
    allowCustomization: true,
    maxItems: 100,
    requireConfirmation: true
  };
  
  @Output() operationStarted = new EventEmitter<void>();
  @Output() operationCompleted = new EventEmitter<BulkOperationResult>();
  @Output() operationFailed = new EventEmitter<string>();
  @Output() itemRemoved = new EventEmitter<string>();

  operationForm!: FormGroup;
  isProcessing = false;
  operationStatus = 'Ready';
  overallProgress = 0;
  operationResult?: BulkOperationResult;
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private apiService: ApiService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.validateSelection();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    this.operationForm = this.formBuilder.group({
      operationType: ['generate', Validators.required],
      priority: ['normal', Validators.required],
      batchSize: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
      batchDelay: [500, [Validators.required, Validators.min(0), Validators.max(5000)]],
      continueOnError: [true],
      sendNotifications: [false],
      saveToHistory: [true]
    });
  }

  private validateSelection(): void {
    if (this.config.maxItems && this.selectedItems.length > this.config.maxItems) {
      this.notificationService.showWarning(
        'Selection Limit Exceeded',
        `Maximum ${this.config.maxItems} items allowed. Some items will be ignored.`
      );
      this.selectedItems = this.selectedItems.slice(0, this.config.maxItems);
    }
  }

  canStartOperation(): boolean {
    return this.selectedItems.length > 0 && 
           this.operationForm.valid && 
           !this.isProcessing;
  }

  async startOperation(): Promise<void> {
    if (!this.canStartOperation()) return;

    if (this.config.requireConfirmation) {
      const confirmed = await this.showConfirmationDialog();
      if (!confirmed) return;
    }

    this.isProcessing = true;
    this.operationStatus = 'Initializing...';
    this.overallProgress = 0;
    this.operationResult = undefined;

    // Reset item statuses
    this.selectedItems.forEach(item => {
      item.status = 'pending';
      item.progress = 0;
      item.error = undefined;
    });

    this.operationStarted.emit();

    try {
      await this.processOperation();
    } catch (error) {
      console.error('Operation failed:', error);
      this.operationFailed.emit(error as string);
      this.notificationService.showError(
        'Operation Failed',
        'An error occurred during bulk operation'
      );
    } finally {
      this.isProcessing = false;
      this.operationStatus = 'Operation completed';
    }
  }

  private async processOperation(): Promise<void> {
    const operationType = this.operationForm.get('operationType')?.value;
    const batchSize = this.operationForm.get('batchSize')?.value;
    const batchDelay = this.operationForm.get('batchDelay')?.value;
    const continueOnError = this.operationForm.get('continueOnError')?.value;

    const totalItems = this.selectedItems.length;
    let processedItems = 0;
    let successfulItems = 0;
    let failedItems = 0;

    // Process items in batches
    for (let i = 0; i < totalItems; i += batchSize) {
      const batch = this.selectedItems.slice(i, i + batchSize);
      
      this.operationStatus = `Processing batch ${Math.floor(i / batchSize) + 1}...`;
      
      // Process batch items
      for (const item of batch) {
        try {
          item.status = 'processing';
          await this.processItem(item, operationType);
          item.status = 'completed';
          item.progress = 100;
          successfulItems++;
        } catch (error) {
          item.status = 'failed';
          item.error = error as string;
          failedItems++;
          
          if (!continueOnError) {
            throw new Error(`Operation stopped due to item failure: ${item.name}`);
          }
        }
        
        processedItems++;
        this.overallProgress = Math.round((processedItems / totalItems) * 100);
        
        // Update progress for individual items
        item.progress = Math.round((processedItems / totalItems) * 100);
      }
      
      // Delay between batches
      if (i + batchSize < totalItems && batchDelay > 0) {
        await new Promise(resolve => setTimeout(resolve, batchDelay));
      }
    }

    // Create operation result
    this.operationResult = {
      totalItems,
      successfulItems,
      failedItems,
      results: [...this.selectedItems],
      summary: `Successfully processed ${successfulItems} out of ${totalItems} items.`
    };

    this.operationCompleted.emit(this.operationResult);
    
    this.notificationService.showSuccess(
      'Operation Completed',
      this.operationResult.summary
    );
  }

  private async processItem(item: BulkOperationItem, operationType: string): Promise<void> {
    // Simulate processing time
    const processingTime = Math.random() * 2000 + 500;
    const startTime = Date.now();
    
    while (Date.now() - startTime < processingTime) {
      const elapsed = Date.now() - startTime;
      item.progress = Math.round((elapsed / processingTime) * 100);
      await new Promise(resolve => setTimeout(resolve, 100));
    }

    // Simulate potential failures
    if (Math.random() < 0.1) { // 10% failure rate
      throw new Error('Simulated processing error');
    }

    // TODO: Implement actual processing logic based on operation type
    switch (operationType) {
      case 'generate':
        // Generate letter logic
        break;
      case 'send':
        // Send email logic
        break;
      case 'update':
        // Update status logic
        break;
      case 'export':
        // Export data logic
        break;
    }
  }

  pauseOperation(): void {
    this.operationStatus = 'Paused';
    // TODO: Implement pause logic
  }

  stopOperation(): void {
    this.operationStatus = 'Stopped';
    this.isProcessing = false;
    // TODO: Implement stop logic
  }

  clearSelection(): void {
    this.selectedItems = [];
    this.operationResult = undefined;
    this.overallProgress = 0;
  }

  invertSelection(): void {
    // TODO: Implement selection inversion logic
    this.notificationService.showInfo(
      'Selection Inverted',
      'Selection inversion feature coming soon'
    );
  }

  removeItem(itemId: string): void {
    this.selectedItems = this.selectedItems.filter(item => item.id !== itemId);
    this.itemRemoved.emit(itemId);
  }

  exportResults(): void {
    if (!this.operationResult) return;
    
    // TODO: Implement export logic
    this.notificationService.showInfo(
      'Export Started',
      'Results export feature coming soon'
    );
  }

  viewDetails(): void {
    if (!this.operationResult) return;
    
    // TODO: Implement details view
    this.notificationService.showInfo(
      'Details View',
      'Detailed results view coming soon'
    );
  }

  private async showConfirmationDialog(): Promise<boolean> {
    // TODO: Implement confirmation dialog
    return confirm(`Are you sure you want to process ${this.selectedItems.length} items?`);
  }
}
