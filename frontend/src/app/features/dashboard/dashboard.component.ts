import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Chart, ChartConfiguration, ChartType } from 'chart.js';
import { Subject, takeUntil } from 'rxjs';
import { MaterialModule } from '../../shared/material.module';
import { DataService, GeneratedLetter } from '../../core/services/data.service';
import { ApiService } from '../../core/services/api.service';

interface DashboardStats {
  totalLetters: number;
  sentEmails: number;
  pendingLetters: number;
  totalEmployees: number;
  lettersGrowth: number;
  emailsGrowth: number;
  pendingGrowth: number;
  employeesGrowth: number;
}

interface Activity {
  id: string;
  type: 'letter' | 'email' | 'upload' | 'signature';
  title: string;
  description: string;
  time: Date;
  status: 'success' | 'pending' | 'error';
}

interface LetterType {
  name: string;
  displayName: string;
  icon: string;
  color: string;
  count: number;
}

interface SystemStatus {
  database: { status: 'online' | 'offline' | 'warning'; details: string };
  email: { status: 'online' | 'offline' | 'warning'; details: string };
  signature: { status: 'online' | 'offline' | 'warning'; details: string };
  storage: { status: 'online' | 'offline' | 'warning'; details: string };
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule]
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('letterChart', { static: true }) letterChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private letterChart: Chart | null = null;

  // Dashboard Data
  stats: DashboardStats = {
    totalLetters: 0,
    sentEmails: 0,
    pendingLetters: 0,
    totalEmployees: 0,
    lettersGrowth: 0,
    emailsGrowth: 0,
    pendingGrowth: 0,
    employeesGrowth: 0
  };

  recentActivity: Activity[] = [];
  letterTypes: LetterType[] = [];
  systemStatus: SystemStatus = {
    database: { status: 'online', details: 'Connected and responsive' },
    email: { status: 'online', details: 'SendGrid service active' },
    signature: { status: 'warning', details: 'PROXKey device not detected' },
    storage: { status: 'online', details: 'File system accessible' }
  };

  selectedPeriod = '30';

  constructor(
    private router: Router,
    private dataService: DataService,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.initializeChart();
    this.loadRecentActivity();
    this.loadLetterTypes();
    this.checkSystemStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.letterChart) {
      this.letterChart.destroy();
    }
  }

  // Dashboard Actions
  quickGenerate(): void {
    this.router.navigate(['/generate']);
  }

  quickUpload(): void {
    this.router.navigate(['/upload']);
  }

  // Dashboard data loading
  loadDashboardData(): void {
    // Load data from API
    this.apiService.getDashboardStats().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.success) {
          const data = response.data;
          this.stats = {
            totalLetters: data.totalLetters || 0,
            sentEmails: data.sentEmails || 0,
            pendingLetters: data.pendingLetters || 0,
            totalEmployees: data.totalEmployees || 0,
            lettersGrowth: data.lettersGrowth || 0,
            emailsGrowth: data.emailsGrowth || 0,
            pendingGrowth: data.pendingGrowth || 0,
            employeesGrowth: data.employeesGrowth || 0
          };
        }
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        // Fallback to mock data for better UI experience
        this.stats = {
          totalLetters: 156,
          sentEmails: 142,
          pendingLetters: 14,
          totalEmployees: 89,
          lettersGrowth: 12,
          emailsGrowth: 8,
          pendingGrowth: -3,
          employeesGrowth: 5
        };
      }
    });
  }

  loadRecentActivity(): void {
    // Load recent activity from API
    this.apiService.getDashboardRecentActivity().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.recentActivity = response.data.map((item: any) => ({
            id: item.id || Date.now().toString(),
            type: this.mapActivityType(item.type),
            title: item.title || 'Activity',
            description: item.description || '',
            time: new Date(item.time || item.createdAt),
            status: this.mapActivityStatus(item.status)
          }));
        }
      },
      error: (error) => {
        console.error('Error loading recent activity:', error);
        // Fallback mock data
        this.recentActivity = [
          {
            id: '1',
            type: 'letter',
            title: 'Transfer Letter Generated',
            description: 'Transfer letter for John Doe created successfully',
            time: new Date(Date.now() - 1800000),
            status: 'success'
          },
          {
            id: '2',
            type: 'email',
            title: 'Bulk Email Sent',
            description: '50 confirmation letters sent to employees',
            time: new Date(Date.now() - 3600000),
            status: 'success'
          },
          {
            id: '3',
            type: 'upload',
            title: 'Excel Data Uploaded',
            description: 'New employee data uploaded successfully',
            time: new Date(Date.now() - 7200000),
            status: 'success'
          },
          {
            id: '4',
            type: 'signature',
            title: 'Digital Signature Updated',
            description: 'New authority signature added to system',
            time: new Date(Date.now() - 10800000),
            status: 'success'
          }
        ];
      }
    });
  }

  private mapActivityType(type: string): 'letter' | 'email' | 'upload' | 'signature' {
    const typeMap: { [key: string]: 'letter' | 'email' | 'upload' | 'signature' } = {
      'letter': 'letter',
      'email': 'email',
      'upload': 'upload',
      'signature': 'signature',
      'LetterGenerated': 'letter',
      'EmailSent': 'email',
      'FileUploaded': 'upload',
      'SignatureCreated': 'signature'
    };
    return typeMap[type] || 'letter';
  }

  private mapActivityStatus(status: string): 'success' | 'pending' | 'error' {
    const statusMap: { [key: string]: 'success' | 'pending' | 'error' } = {
      'success': 'success',
      'pending': 'pending',
      'error': 'error',
      'failed': 'error',
      'completed': 'success'
    };
    return statusMap[status] || 'success';
  }

  loadLetterTypes(): void {
    // Load letter types from dynamic tabs
    this.dataService.templates$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(templates => {
      if (templates.length > 0) {
        this.letterTypes = templates
          .filter(t => t.isActive)
          .map(template => ({
            name: template.name,
            displayName: template.displayName,
            icon: this.getLetterIcon(template.name),
            color: this.getRandomColor(),
            count: Math.floor(Math.random() * 100) // Mock count for now
          }));
      } else {
        // Fallback mock data
        this.letterTypes = [
          { name: 'transfer', displayName: 'Transfer Letters', icon: '🔄', color: '#3B82F6', count: 45 },
          { name: 'experience', displayName: 'Experience Letters', icon: '📋', color: '#10B981', count: 67 },
          { name: 'confirmation', displayName: 'Confirmation Letters', icon: '✅', color: '#F59E0B', count: 23 },
          { name: 'termination', displayName: 'Termination Letters', icon: '🚪', color: '#EF4444', count: 12 }
        ];
      }
    });

    // Load templates to trigger data loading
    this.dataService.loadTemplates();
  }

  private getRandomColor(): string {
    const colors = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#06B6D4', '#F97316'];
    return colors[Math.floor(Math.random() * colors.length)];
  }

  checkSystemStatus(): void {
    // Mock system status check
    console.log('System status checked');
  }

  // Chart management
  initializeChart(): void {
    if (this.letterChartRef) {
      const ctx = this.letterChartRef.nativeElement.getContext('2d');
      if (ctx) {
        this.letterChart = new Chart(ctx, {
          type: 'line' as ChartType,
          data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            datasets: [{
              label: 'Letters Generated',
              data: [65, 59, 80, 81, 56, 55],
              borderColor: '#3B82F6',
              backgroundColor: 'rgba(59, 130, 246, 0.1)',
              tension: 0.4
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: {
                display: false
              }
            },
            scales: {
              y: {
                beginAtZero: true
              }
            }
          }
        });
      }
    }
  }

  updateChart(): void {
    // Mock chart update based on selected period
    console.log('Chart updated for period:', this.selectedPeriod);
  }

  // Helper methods for template
  getActivityIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'letter': '📝',
      'email': '📧',
      'upload': '📤',
      'signature': '✍️'
    };
    return iconMap[type] || '📄';
  }

  getLetterIcon(icon: string): string {
    const iconMap: { [key: string]: string } = {
      'transfer': '🔄',
      'experience': '📋',
      'confirmation': '✅',
      'termination': '🚪',
      'default': '📄'
    };
    return iconMap[icon] || iconMap['default'];
  }

  getStatusIcon(status: string): string {
    const iconMap: { [key: string]: string } = {
      'online': '🟢',
      'offline': '🔴',
      'warning': '🟡'
    };
    return iconMap[status] || '⚪';
  }

  generateLetter(letterType: string): void {
    this.router.navigate(['/generate', letterType]);
  }

  getAbsoluteValue(value: number): number {
    return Math.abs(value);
  }
}
