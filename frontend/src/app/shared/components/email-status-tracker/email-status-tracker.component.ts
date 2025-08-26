import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { Subject, takeUntil, filter } from 'rxjs';
import { SignalRService, EmailStatusUpdate, BulkEmailProgress } from '../../../core/services/signalr.service';

export interface EmailStatus {
  emailId: string;
  status: 'pending' | 'sending' | 'sent' | 'delivered' | 'failed' | 'retrying';
  message: string;
  timestamp: Date;
  errorDetails?: string;
  retryCount: number;
  nextRetryTime?: Date;
}

@Component({
  selector: 'app-email-status-tracker',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatChipsModule,
    MatTooltipModule,
    MatExpansionModule
  ],
  template: `
    <div class="email-status-tracker">
      <!-- Connection Status -->
      <div class="connection-status" [class.connected]="isConnected">
        <mat-icon>{{ isConnected ? 'wifi' : 'wifi_off' }}</mat-icon>
        <span>{{ isConnected ? 'Connected' : 'Disconnected' }}</span>
        <button mat-button color="primary" (click)="reconnect()" *ngIf="!isConnected">
          <mat-icon>refresh</mat-icon>
          Reconnect
        </button>
      </div>

      <!-- Real-time Updates -->
      <mat-card class="status-card" *ngIf="emailStatuses.length > 0">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>email</mat-icon>
            Email Status Updates
          </mat-card-title>
          <mat-card-subtitle>
            Real-time tracking of email delivery status
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="status-list">
            <div *ngFor="let status of emailStatuses.slice(0, 5)" 
                 class="status-item" [class]="status.status">
              <div class="status-header">
                <span class="status-icon">
                  <mat-icon>{{ getStatusIcon(status.status) }}</mat-icon>
                </span>
                <span class="status-text">{{ status.status | titlecase }}</span>
                <span class="status-time">{{ status.timestamp | date:'shortTime' }}</span>
              </div>
              
              <div class="status-message">{{ status.message }}</div>
              
              <div class="status-details" *ngIf="status.errorDetails || status.retryCount > 0">
                <span class="retry-count" *ngIf="status.retryCount > 0">
                  Retry: {{ status.retryCount }}
                </span>
                <span class="error-details" *ngIf="status.errorDetails">
                  Error: {{ status.errorDetails }}
                </span>
                <span class="next-retry" *ngIf="status.nextRetryTime">
                  Next retry: {{ status.nextRetryTime | date:'shortTime' }}
                </span>
              </div>
            </div>
          </div>

          <mat-expansion-panel *ngIf="emailStatuses.length > 5" class="more-statuses">
            <mat-expansion-panel-header>
              <mat-panel-title>
                View All Updates ({{ emailStatuses.length }})
              </mat-panel-title>
            </mat-expansion-panel-header>
            
            <div class="status-list">
              <div *ngFor="let status of emailStatuses.slice(5)" 
                   class="status-item" [class]="status.status">
                <div class="status-header">
                  <span class="status-icon">
                    <mat-icon>{{ getStatusIcon(status.status) }}</mat-icon>
                  </span>
                  <span class="status-text">{{ status.status | titlecase }}</span>
                  <span class="status-time">{{ status.timestamp | date:'shortTime' }}</span>
                </div>
                
                <div class="status-message">{{ status.message }}</div>
                
                <div class="status-details" *ngIf="status.errorDetails || status.retryCount > 0">
                  <span class="retry-count" *ngIf="status.retryCount > 0">
                    Retry: {{ status.retryCount }}
                  </span>
                  <span class="error-details" *ngIf="status.errorDetails">
                    Error: {{ status.errorDetails }}
                  </span>
                  <span class="next-retry" *ngIf="status.nextRetryTime">
                    Next retry: {{ status.nextRetryTime | date:'shortTime' }}
                  </span>
                </div>
              </div>
            </div>
          </mat-expansion-panel>
        </mat-card-content>
      </mat-card>

      <!-- Bulk Email Progress -->
      <mat-card class="progress-card" *ngIf="bulkEmailProgress">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>batch_prediction</mat-icon>
            Bulk Email Progress
          </mat-card-title>
          <mat-card-subtitle>
            Processing {{ bulkEmailProgress.current }} of {{ bulkEmailProgress.total }} emails
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="progress-info">
            <div class="progress-stats">
              <span class="stat-item">
                <mat-icon>check_circle</mat-icon>
                {{ bulkEmailProgress.successful }} Successful
              </span>
              <span class="stat-item">
                <mat-icon>schedule</mat-icon>
                {{ bulkEmailProgress.total - bulkEmailProgress.current }} Remaining
              </span>
            </div>
            
            <mat-progress-bar 
              mode="determinate" 
              [value]="(bulkEmailProgress.current / bulkEmailProgress.total) * 100"
              class="progress-bar">
            </mat-progress-bar>
            
            <div class="progress-text">
              {{ (bulkEmailProgress.current / bulkEmailProgress.total) * 100 | number:'1.0-0' }}% Complete
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- No Updates Message -->
      <div class="no-updates" *ngIf="emailStatuses.length === 0 && !bulkEmailProgress">
        <mat-icon>email</mat-icon>
        <span>No email updates yet</span>
        <p>Email status updates will appear here in real-time</p>
      </div>
    </div>
  `,
  styleUrls: ['./email-status-tracker.component.scss']
})
export class EmailStatusTrackerComponent implements OnInit, OnDestroy {
  @Input() maxStatuses = 10;

  isConnected = false;
  emailStatuses: EmailStatus[] = [];
  bulkEmailProgress: BulkEmailProgress | null = null;

  private destroy$ = new Subject<void>();

  constructor(private signalRService: SignalRService) {}

  ngOnInit() {
    this.setupSignalRSubscriptions();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSignalRSubscriptions() {
    // Connection status
    this.signalRService.connectionEstablished$
      .pipe(takeUntil(this.destroy$))
      .subscribe(connected => {
        this.isConnected = connected;
      });

    // Email status updates
    this.signalRService.emailStatusUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        this.addEmailStatus(update);
      });

    // Bulk email started
    this.signalRService.bulkEmailStarted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(total => {
        this.bulkEmailProgress = {
          current: 0,
          total,
          successful: 0
        };
      });

    // Bulk email progress
    this.signalRService.bulkEmailProgress$
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.bulkEmailProgress = progress;
      });

    // Bulk email completed
    this.signalRService.bulkEmailCompleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ total, successful }) => {
        this.bulkEmailProgress = null;
        // Add completion status
        this.addEmailStatus({
          emailId: 'bulk-completion',
          status: 'delivered',
          message: `Bulk email completed: ${successful} of ${total} emails sent successfully`,
          timestamp: new Date(),
          retryCount: 0
        });
      });
  }

  private addEmailStatus(update: EmailStatusUpdate) {
    const status: EmailStatus = {
      emailId: update.emailId,
      status: update.status as EmailStatus['status'],
      message: update.message,
      timestamp: update.timestamp,
      errorDetails: update.errorDetails,
      retryCount: update.retryCount,
      nextRetryTime: update.nextRetryTime
    };

    this.emailStatuses.unshift(status);

    // Keep only the latest statuses
    if (this.emailStatuses.length > this.maxStatuses) {
      this.emailStatuses = this.emailStatuses.slice(0, this.maxStatuses);
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'pending':
        return 'schedule';
      case 'sending':
        return 'send';
      case 'sent':
        return 'check_circle';
      case 'delivered':
        return 'done_all';
      case 'failed':
        return 'error';
      case 'retrying':
        return 'refresh';
      default:
        return 'email';
    }
  }

  async reconnect() {
    await this.signalRService.reconnect();
  }

  clearStatuses() {
    this.emailStatuses = [];
  }
}
