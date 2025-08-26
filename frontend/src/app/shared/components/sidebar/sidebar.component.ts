import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatSlideToggleModule,
    MatTooltipModule,
    FormsModule
  ],
  template: `
    <aside class="sidebar" [class.is-open]="isOpen">
      <div class="sidebar-header">
        <div class="sidebar-title">
          <span class="title-icon">📚</span>
          <span class="title-text">Navigation</span>
        </div>
      </div>

      <nav class="sidebar-nav">
        <div class="nav-section">
          <div class="nav-section-title">Main</div>
          <a 
            *ngFor="let item of mainMenuItems" 
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
            [matTooltip]="item.tooltip">
            <mat-icon class="nav-icon">{{ item.icon }}</mat-icon>
            <span class="nav-text">{{ item.label }}</span>
            <span *ngIf="item.badge" class="nav-badge">{{ item.badge }}</span>
          </a>
        </div>

        <mat-divider class="nav-divider"></mat-divider>

        <div class="nav-section">
          <div class="nav-section-title">Letter Management</div>
          <a 
            *ngFor="let item of letterMenuItems" 
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
            [matTooltip]="item.tooltip">
            <mat-icon class="nav-icon">{{ item.icon }}</mat-icon>
            <span class="nav-text">{{ item.label }}</span>
            <span *ngIf="item.badge" class="nav-badge">{{ item.badge }}</span>
          </a>
        </div>

        <mat-divider class="nav-divider"></mat-divider>

        <div class="nav-section">
          <div class="nav-section-title">Administration</div>
          <a 
            *ngFor="let item of adminMenuItems" 
            [routerLink]="item.route"
            routerLinkActive="active"
            class="nav-item"
            [matTooltip]="item.tooltip">
            <mat-icon class="nav-icon">{{ item.icon }}</mat-icon>
            <span class="nav-text">{{ item.label }}</span>
            <span *ngIf="item.badge" class="nav-badge">{{ item.badge }}</span>
          </a>
        </div>
      </nav>

      <div class="sidebar-footer">
        <div class="theme-toggle">
          <mat-icon class="theme-icon">{{ isDarkTheme ? 'light_mode' : 'dark_mode' }}</mat-icon>
          <span class="theme-text">Dark Mode</span>
          <mat-slide-toggle
            [checked]="isDarkTheme"
            (change)="onThemeToggle($event.checked)"
            class="theme-slider">
          </mat-slide-toggle>
        </div>
      </div>
    </aside>
  `,
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Input() isOpen = true;

  private themeService = inject(ThemeService);
  private router = inject(Router);
  isDarkTheme = false;

  mainMenuItems = [
    {
      label: 'Dashboard',
      route: '/dashboard',
      icon: 'dashboard',
      tooltip: 'Main dashboard overview',
      badge: null
    },
    {
      label: 'Overview',
      route: '/overview',
      icon: 'visibility',
      tooltip: 'System overview and statistics',
      badge: null
    }
  ];

  letterMenuItems = [
    {
      label: 'Letter Templates',
      route: '/templates',
      icon: 'description',
      tooltip: 'Manage letter templates',
      badge: null
    },
    {
      label: 'Generate Letters',
      route: '/generate/transfer-letter',
      icon: 'create',
      tooltip: 'Generate new letters',
      badge: 'New'
    },
    {
      label: 'Letter History',
      route: '/history',
      icon: 'history',
      tooltip: 'View letter generation history',
      badge: null
    },
    {
      label: 'Email Status',
      route: '/email-status',
      icon: 'email',
      tooltip: 'Monitor email delivery status',
      badge: '3'
    }
  ];

  adminMenuItems = [
    {
      label: 'Employees',
      route: '/employees',
      icon: 'people',
      tooltip: 'Manage employee data',
      badge: null
    },
    {
      label: 'Digital Signatures',
      route: '/signatures',
      icon: 'draw',
      tooltip: 'Manage digital signatures',
      badge: null
    },
    {
      label: 'PROXKey Device',
      route: '/proxkey',
      icon: 'usb',
      tooltip: 'Manage PROXKey digital signature device',
      badge: 'New'
    },
    {
      label: 'Upload Data',
      route: '/upload',
      icon: 'upload_file',
      tooltip: 'Upload Excel data',
      badge: null
    },
    {
      label: 'Admin Dashboard',
      route: '/admin/tabs',
      icon: 'admin_panel_settings',
      tooltip: 'System administration',
      badge: null
    },
    {
      label: 'Settings',
      route: '/settings',
      icon: 'settings',
      tooltip: 'System settings',
      badge: null
    }
  ];

  ngOnInit() {
    this.themeService.isDarkTheme$.subscribe(isDark => {
      this.isDarkTheme = isDark;
    });
  }

  onThemeToggle(isDark: boolean) {
    this.themeService.setTheme(isDark);
  }
}
