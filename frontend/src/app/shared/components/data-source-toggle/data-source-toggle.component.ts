import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';

import { DataSourceService, DataSourceConfiguration } from '../../../core/services/data-source.service';
import { NotificationService } from '../../../core/services/notification.service';

export interface DataSourceToggleConfig {
  showLabel?: boolean;
  showDescription?: boolean;
  showValidation?: boolean;
  allowBulkToggle?: boolean;
  compact?: boolean;
}

@Component({
  selector: 'app-data-source-toggle',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDialogModule
  ],
  template: `
    <div class="data-source-toggle" [class.compact]="config.compact">
      
      <!-- Single Item Toggle -->
      <div *ngIf="itemId && !config.allowBulkToggle" class="single-toggle">
        <div class="toggle-header" *ngIf="config.showLabel">
          <h4 class="toggle-title">Data Source Configuration</h4>
          <p class="toggle-description" *ngIf="config.showDescription">
            Choose whether to use uploaded data or database records for this template
          </p>
        </div>

        <div class="toggle-content">
          <div class="current-source" *ngIf="currentConfiguration">
            <div class="source-info">
              <mat-chip 
                [color]="getSourceColor(currentConfiguration.dataSource)" 
                selected>
                <mat-icon>{{ getSourceIcon(currentConfiguration.dataSource) }}</mat-icon>
                {{ getSourceLabel(currentConfiguration.dataSource) }}
              </mat-chip>
              <span class="last-updated" *ngIf="!config.compact">
                Last updated: {{ currentConfiguration.lastUpdated | date:'short' }}
              </span>
            </div>
          </div>

          <div class="toggle-actions">
            <mat-form-field appearance="outline" class="source-select">
              <mat-label>Data Source</mat-label>
              <mat-select 
                [(value)]="selectedSource" 
                (selectionChange)="onSourceChange($event.value)"
                [disabled]="isLoading">
                <mat-option value="upload">
                  <mat-icon>cloud_upload</mat-icon>
                  Uploaded Data
                </mat-option>
                <mat-option value="database">
                  <mat-icon>storage</mat-icon>
                  Database Records
                </mat-option>
              </mat-select>
            </mat-form-field>

            <button 
              mat-raised-button 
              color="primary"
              [disabled]="!hasChanges() || isLoading"
              (click)="applyChanges()"
              class="apply-btn">
              <mat-icon *ngIf="!isLoading">save</mat-icon>
              <mat-spinner *ngIf="isLoading" diameter="16"></mat-spinner>
              {{ isLoading ? 'Applying...' : 'Apply Changes' }}
            </button>
          </div>

          <!-- Validation Results -->
          <div class="validation-results" *ngIf="config.showValidation && validationResults">
            <div class="validation-status" [class.valid]="validationResults.valid" [class.invalid]="!validationResults.valid">
              <mat-icon>{{ validationResults.valid ? 'check_circle' : 'warning' }}</mat-icon>
              <span>{{ validationResults.valid ? 'Configuration is valid' : 'Issues detected' }}</span>
            </div>
            <div class="validation-issues" *ngIf="!validationResults.valid">
              <ul>
                <li *ngFor="let issue of validationResults.issues">{{ issue }}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <!-- Bulk Toggle Panel -->
      <div *ngIf="config.allowBulkToggle" class="bulk-toggle">
        <div class="bulk-header">
          <h4 class="bulk-title">Bulk Data Source Management</h4>
          <p class="bulk-description" *ngIf="config.showDescription">
            Manage data sources for multiple templates simultaneously
          </p>
        </div>

        <div class="bulk-stats">
          <div class="stat-card">
            <mat-icon>upload</mat-icon>
            <div class="stat-info">
              <span class="stat-label">Upload Sources</span>
              <span class="stat-value">{{ stats.uploadSources }}</span>
            </div>
          </div>
          <div class="stat-card">
            <mat-icon>storage</mat-icon>
            <div class="stat-info">
              <span class="stat-label">Database Sources</span>
              <span class="stat-value">{{ stats.databaseSources }}</span>
            </div>
          </div>
          <div class="stat-card">
            <mat-icon>check_circle</mat-icon>
            <div class="stat-info">
              <span class="stat-label">Active Sources</span>
              <span class="stat-value">{{ stats.activeSources }}</span>
            </div>
          </div>
        </div>

        <div class="bulk-actions">
          <button 
            mat-stroked-button 
            (click)="bulkToggle('upload')"
            [disabled]="isLoading"
            class="bulk-btn upload">
            <mat-icon>cloud_upload</mat-icon>
            Set All to Upload
          </button>
          <button 
            mat-stroked-button 
            (click)="bulkToggle('database')"
            [disabled]="isLoading"
            class="bulk-btn database">
            <mat-icon>storage</mat-icon>
            Set All to Database
          </button>
          <button 
            mat-stroked-button 
            (click)="refreshConfigurations()"
            [disabled]="isLoading"
            class="refresh-btn">
            <mat-icon>refresh</mat-icon>
            Refresh
          </button>
        </div>
      </div>

      <!-- Configuration List -->
      <div class="configuration-list" *ngIf="config.allowBulkToggle && configurations.length > 0">
        <div class="list-header">
          <h5>Template Configurations</h5>
        </div>
        <div class="configuration-items">
          <div 
            *ngFor="let configuration of configurations" 
            class="configuration-item"
            [class.active]="configuration.isActive">
            <div class="item-info">
              <div class="item-name">{{ configuration.name }}</div>
              <div class="item-description" *ngIf="configuration.description">
                {{ configuration.description }}
              </div>
              <div class="item-meta">
                <span class="item-type">{{ configuration.type }}</span>
                <span class="item-updated">{{ configuration.lastUpdated | date:'short' }}</span>
              </div>
            </div>
            <div class="item-source">
              <mat-chip 
                [color]="getSourceColor(configuration.dataSource)" 
                selected
                class="source-chip">
                <mat-icon>{{ getSourceIcon(configuration.dataSource) }}</mat-icon>
                {{ getSourceLabel(configuration.dataSource) }}
              </mat-chip>
            </div>
            <div class="item-actions">
              <button 
                mat-icon-button 
                [matTooltip]="'Switch to ' + (configuration.dataSource === 'upload' ? 'database' : 'upload')"
                (click)="toggleSingleConfiguration(configuration)"
                [disabled]="isLoading">
                <mat-icon>swap_horiz</mat-icon>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./data-source-toggle.component.scss']
})
export class DataSourceToggleComponent implements OnInit {
  @Input() itemId?: string;
  @Input() config: DataSourceToggleConfig = {
    showLabel: true,
    showDescription: true,
    showValidation: false,
    allowBulkToggle: false,
    compact: false
  };
  
  @Output() sourceChanged = new EventEmitter<{itemId: string, newSource: 'upload' | 'database'}>();
  @Output() bulkChanged = new EventEmitter<{targetSource: 'upload' | 'database', affectedIds: string[]}>();

  currentConfiguration?: DataSourceConfiguration;
  configurations: DataSourceConfiguration[] = [];
  selectedSource: 'upload' | 'database' = 'upload';
  isLoading = false;
  validationResults?: {valid: boolean, issues: string[]};
  stats = {
    totalSources: 0,
    uploadSources: 0,
    databaseSources: 0,
    activeSources: 0
  };

  constructor(
    private dataSourceService: DataSourceService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadConfigurations();
    this.subscribeToStats();
  }

  private loadConfigurations(): void {
    this.dataSourceService.configurations$.subscribe(configs => {
      this.configurations = configs;
      
      if (this.itemId) {
        this.currentConfiguration = configs.find(c => c.id === this.itemId);
        if (this.currentConfiguration) {
          this.selectedSource = this.currentConfiguration.dataSource;
        }
      }
    });
  }

  private subscribeToStats(): void {
    this.dataSourceService.stats$.subscribe(stats => {
      this.stats = stats;
    });
  }

  onSourceChange(newSource: 'upload' | 'database'): void {
    this.selectedSource = newSource;
    
    if (this.config.showValidation && this.itemId) {
      this.validateConfiguration(newSource);
    }
  }

  hasChanges(): boolean {
    return this.currentConfiguration?.dataSource !== this.selectedSource;
  }

  applyChanges(): void {
    if (!this.itemId || !this.hasChanges()) return;

    this.isLoading = true;
    this.dataSourceService.toggleDataSource(this.itemId, this.selectedSource).subscribe({
      next: (success) => {
        if (success) {
          this.sourceChanged.emit({
            itemId: this.itemId!,
            newSource: this.selectedSource
          });
          // Update current configuration
          if (this.currentConfiguration) {
            this.currentConfiguration.dataSource = this.selectedSource;
            this.currentConfiguration.lastUpdated = new Date();
          }
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error toggling data source:', error);
        this.notificationService.showError(
          'Toggle Failed',
          'Failed to update data source configuration'
        );
        this.isLoading = false;
      }
    });
  }

  bulkToggle(targetSource: 'upload' | 'database'): void {
    const activeIds = this.configurations
      .filter(c => c.isActive)
      .map(c => c.id);

    if (activeIds.length === 0) {
      this.notificationService.showWarning(
        'No Active Templates',
        'No active templates found to update'
      );
      return;
    }

    this.isLoading = true;
    this.dataSourceService.bulkToggleDataSource(activeIds, targetSource).subscribe({
      next: (success) => {
        if (success) {
          this.bulkChanged.emit({
            targetSource,
            affectedIds: activeIds
          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error in bulk toggle:', error);
        this.isLoading = false;
      }
    });
  }

  toggleSingleConfiguration(configuration: DataSourceConfiguration): void {
    const newSource = configuration.dataSource === 'upload' ? 'database' : 'upload';
    
    this.isLoading = true;
    this.dataSourceService.toggleDataSource(configuration.id, newSource).subscribe({
      next: (success) => {
        if (success) {
          this.sourceChanged.emit({
            itemId: configuration.id,
            newSource
          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error toggling configuration:', error);
        this.isLoading = false;
      }
    });
  }

  refreshConfigurations(): void {
    this.dataSourceService.refresh();
  }

  private validateConfiguration(targetSource: 'upload' | 'database'): void {
    if (!this.itemId) return;

    this.dataSourceService.validateDataSourceCompatibility(this.itemId, targetSource).subscribe({
      next: (results) => {
        this.validationResults = results;
      },
      error: (error) => {
        console.error('Error validating configuration:', error);
      }
    });
  }

  getSourceColor(source: 'upload' | 'database'): 'primary' | 'accent' {
    return source === 'upload' ? 'primary' : 'accent';
  }

  getSourceIcon(source: 'upload' | 'database'): string {
    return source === 'upload' ? 'cloud_upload' : 'storage';
  }

  getSourceLabel(source: 'upload' | 'database'): string {
    return source === 'upload' ? 'Upload' : 'Database';
  }
}
