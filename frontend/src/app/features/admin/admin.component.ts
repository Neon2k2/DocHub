import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Subject, takeUntil, interval } from 'rxjs';

import { ApiService, ApiResponse } from '../../core/services/api.service';

interface DashboardStats {
  totalTemplates: number;
  activeTemplates: number;
  totalEmployees: number;
  activeEmployees: number;
  totalLetters: number;
  totalSignatures: number;
  activeSignatures: number;
  lettersGenerated: number;
  lettersSent: number;
  lettersFailed: number;
  lettersThisWeek: number;
  employeesAddedThisWeek: number;
  uploadTemplates: number;
  databaseTemplates: number;
  successRate: number;
}

interface SystemHealth {
  status: 'healthy' | 'warning' | 'error';
  message: string;
  lastChecked: Date;
  services: ServiceStatus[];
}

interface ServiceStatus {
  name: string;
  status: 'online' | 'offline' | 'degraded';
  responseTime: number;
  lastCheck: Date;
}

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    MatDividerModule,
    MatListModule,
    MatBadgeModule,
    MatTooltipModule
  ]
})
export class AdminComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  dashboardStats: DashboardStats | null = null;
  systemHealth: SystemHealth | null = null;
  isLoading = true;
  isRefreshing = false;
  currentDate = new Date();
  
  // Quick action buttons
  quickActions = [
    {
      name: 'Generate Report',
      icon: 'assessment',
      route: '/admin/reports',
      description: 'Generate system reports and analytics'
    },
    {
      name: 'User Management',
      icon: 'people',
      route: '/admin/users',
      description: 'Manage user accounts and permissions'
    },
    {
      name: 'System Settings',
      icon: 'settings',
      route: '/admin/settings',
      description: 'Configure system parameters and integrations'
    },
    {
      name: 'Backup & Restore',
      icon: 'backup',
      route: '/admin/backup',
      description: 'Manage system backups and restoration'
    }
  ];

  constructor(
    private apiService: ApiService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadSystemHealth();
    
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

  loadDashboardData(): void {
    this.isLoading = true;
    this.apiService.getDashboardStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<DashboardStats>) => {
          if (response.success && response.data) {
            this.dashboardStats = response.data;
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard stats:', error);
          this.snackBar.open('Failed to load dashboard statistics', 'Close', { duration: 3000 });
          this.isLoading = false;
        }
      });
  }

  loadSystemHealth(): void {
    this.apiService.getHealthStatus()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success && response.data) {
            this.systemHealth = this.mapHealthResponse(response.data);
          }
        },
        error: (error) => {
          console.error('Error loading system health:', error);
          this.systemHealth = {
            status: 'error',
            message: 'Unable to determine system health',
            lastChecked: new Date(),
            services: []
          };
        }
      });
  }

  refreshData(): void {
    this.isRefreshing = true;
    this.loadDashboardData();
    this.loadSystemHealth();
    
    setTimeout(() => {
      this.isRefreshing = false;
    }, 1000);
  }

  private mapHealthResponse(data: any): SystemHealth {
    return {
      status: data.status || 'healthy',
      message: data.message || 'System is operating normally',
      lastChecked: new Date(data.lastChecked || Date.now()),
      services: data.services || []
    };
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'healthy':
      case 'online':
        return 'primary';
      case 'warning':
      case 'degraded':
        return 'accent';
      case 'error':
      case 'offline':
        return 'warn';
      default:
        return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'healthy':
      case 'online':
        return 'check_circle';
      case 'warning':
      case 'degraded':
        return 'warning';
      case 'error':
      case 'offline':
        return 'error';
      default:
        return 'help';
    }
  }

  navigateToRoute(route: string): void {
    this.router.navigate([route]);
  }

  getProgressValue(current: number, total: number): number {
    if (total === 0) return 0;
    return Math.round((current / total) * 100);
  }

  formatNumber(num: number): string {
    return num.toLocaleString();
  }

  getSuccessRateColor(rate: number): string {
    if (rate >= 90) return 'primary';
    if (rate >= 70) return 'accent';
    return 'warn';
  }
}
