import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';

import { DynamicTabService } from './core/services/dynamic-tab.service';
import { DynamicTabDto } from './core/models/dynamic-tab.dto';

interface Toast {
  id: string;
  type: 'success' | 'warning' | 'error' | 'info';
  message: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule],
  animations: [
    trigger('slideInRight', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ])
    ])
  ]
})
export class AppComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // UI State
  isDarkTheme = false;
  isSidebarCollapsed = false;
  showUserMenu = false;
  isLoading = false;
  activePage = 'history';

  // Data
  dynamicTabs: DynamicTabDto[] = [];
  isAdmin = true; // For now, set to true to show admin features

  // Toast notifications
  toasts: Toast[] = [];

  constructor(
    private router: Router,
    private dynamicTabService: DynamicTabService
  ) {}

  ngOnInit(): void {
    this.loadDynamicTabs();
    this.loadThemePreference();
    this.setupRouteListener();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Theme Management
  toggleTheme(): void {
    this.isDarkTheme = !this.isDarkTheme;
    this.saveThemePreference();
    this.applyTheme();
  }

  private loadThemePreference(): void {
    const savedTheme = localStorage.getItem('dochub-theme');
    this.isDarkTheme = savedTheme === 'dark';
    this.applyTheme();
  }

  private saveThemePreference(): void {
    localStorage.setItem('dochub-theme', this.isDarkTheme ? 'dark' : 'light');
  }

  private applyTheme(): void {
    if (this.isDarkTheme) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }

  // Sidebar Management
  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  // User Menu Management
  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  // Navigation
  setActivePage(page: string): void {
    this.activePage = page;
    this.showUserMenu = false;
  }

  getCurrentPageTitle(): string {
    const pageTitles: { [key: string]: string } = {
      'history': 'Email History',
      'upload': 'Upload Data',
      'generate': 'Generate Letters',
      'templates': 'Letter Templates',
      'signatures': 'Digital Signatures',
      'employees': 'Employee Management',
      'proxkey': 'PROXKey Setup',
      'admin-tabs': 'Manage Dynamic Tabs',
      'admin-settings': 'System Settings'
    };
    return pageTitles[this.activePage] || 'DocHub';
  }

  // Dynamic Tabs
  private loadDynamicTabs(): void {
    this.dynamicTabService.getAllActiveTabs()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (tabs) => {
          this.dynamicTabs = tabs;
        },
        error: (error) => {
          console.error('Error loading dynamic tabs:', error);
          // Load default tabs if API fails
          this.loadDefaultTabs();
        }
      });
  }

  private loadDefaultTabs(): void {
    this.dynamicTabs = [
      {
        id: '1',
        name: 'transfer-letter',
        displayName: 'Transfer Letter',
        description: 'Employee transfer notifications',
        dataSource: 'Upload',
        icon: '🔄',
        color: '#3B82F6',
        sortOrder: 1,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      },
      {
        id: '2',
        name: 'experience-letter',
        displayName: 'Experience Letter',
        description: 'Employee experience certificates',
        dataSource: 'Database',
        icon: '📋',
        color: '#10B981',
        sortOrder: 2,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: 'SELECT * FROM employees WHERE status = "active"'
      },
      {
        id: '3',
        name: 'confirmation-letter',
        displayName: 'Confirmation Letter',
        description: 'Employee confirmation letters',
        dataSource: 'Upload',
        icon: '✅',
        color: '#F59E0B',
        sortOrder: 3,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      },
      {
        id: '4',
        name: 'mutual-cessation-letter',
        displayName: 'Mutual Cessation Letter',
        description: 'Mutual termination agreements',
        dataSource: 'Upload',
        icon: '🚪',
        color: '#EF4444',
        sortOrder: 4,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      }
    ];
  }

  // Route Listener
  private setupRouteListener(): void {
    this.router.events
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        const url = this.router.url;
        if (url.includes('/history')) {
          this.activePage = 'history';
        } else if (url.includes('/upload')) {
          this.activePage = 'upload';
        } else if (url.includes('/generate')) {
          this.activePage = 'generate';
        } else if (url.includes('/templates')) {
          this.activePage = 'templates';
        } else if (url.includes('/signatures')) {
          this.activePage = 'signatures';
        } else if (url.includes('/employees')) {
          this.activePage = 'employees';
        } else if (url.includes('/proxkey')) {
          this.activePage = 'proxkey';
        } else if (url.includes('/admin/tabs')) {
          this.activePage = 'admin-tabs';
        } else if (url.includes('/admin/settings')) {
          this.activePage = 'admin-settings';
        }
      });
  }

  // User Actions
  navigateToSettings(): void {
    this.router.navigate(['/admin/settings']);
    this.showUserMenu = false;
  }

  logout(): void {
    // Implement logout logic here
    console.log('Logout clicked');
    this.showUserMenu = false;
    this.router.navigate(['/login']);
  }

  // Toast Management
  showToast(type: 'success' | 'warning' | 'error' | 'info', message: string): void {
    const toast: Toast = {
      id: Date.now().toString(),
      type,
      message
    };
    
    this.toasts.push(toast);
    
    // Auto-remove toast after 5 seconds
    setTimeout(() => {
      this.removeToast(toast.id);
    }, 5000);
  }

  removeToast(id: string): void {
    this.toasts = this.toasts.filter(toast => toast.id !== id);
  }

  getToastIcon(type: string): string {
    const icons: { [key: string]: string } = {
      success: '✅',
      warning: '⚠️',
      error: '❌',
      info: 'ℹ️'
    };
    return icons[type] || 'ℹ️';
  }

  // Loading State
  setLoading(loading: boolean): void {
    this.isLoading = loading;
  }
}
