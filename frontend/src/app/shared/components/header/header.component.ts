import { Component, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';

import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatMenuModule,

    MatDividerModule,
    MatTooltipModule
  ],
  template: `
    <header class="header">
      <div class="header-left">
        <button 
          mat-icon-button 
          class="menu-toggle"
          (click)="onMenuToggle()"
          matTooltip="Toggle Menu">
          <mat-icon>menu</mat-icon>
        </button>
        
        <div class="logo">
          <span class="logo-icon">📄</span>
          <span class="logo-text">DocHub</span>
        </div>
      </div>

      <div class="header-center">
        <div class="search-container">
          <mat-icon class="search-icon">search</mat-icon>
          <input
            matInput
            type="text"
            placeholder="Search documents, employees, templates..."
            [(ngModel)]="searchQuery"
            (keyup.enter)="onSearch()"
            class="search-input"
            autocomplete="off">
        </div>
      </div>

      <div class="header-right">
        <button 
          mat-icon-button 
          class="theme-toggle"
          (click)="toggleTheme()"
          [matTooltip]="isDarkTheme ? 'Switch to Light Mode' : 'Switch to Dark Mode'">
          <mat-icon>{{ isDarkTheme ? 'light_mode' : 'dark_mode' }}</mat-icon>
        </button>

        <button 
          mat-icon-button 
          class="notification-btn"
          [matMenuTriggerFor]="notificationMenu"
          matTooltip="Notifications">
          <mat-icon>notifications</mat-icon>
          <span 
            *ngIf="notificationCount > 0"
            class="notification-badge">
            {{ notificationCount }}
          </span>
        </button>

        <button 
          mat-icon-button 
          class="profile-btn"
          [matMenuTriggerFor]="profileMenu"
          matTooltip="Profile">
          <div class="avatar">
            <span class="avatar-text">U</span>
          </div>
        </button>
      </div>

      <!-- Notification Menu -->
      <mat-menu #notificationMenu="matMenu" class="notification-menu">
        <div class="menu-header">
          <h3>Notifications</h3>
          <button mat-button color="primary">Mark all as read</button>
        </div>
        <mat-divider></mat-divider>
        <div class="notification-list">
          <div 
            *ngFor="let notification of notifications" 
            class="notification-item"
            [class.unread]="!notification.read">
            <div class="notification-icon">
              <mat-icon [class]="notification.type">{{ notification.icon }}</mat-icon>
            </div>
            <div class="notification-content">
              <div class="notification-title">{{ notification.title }}</div>
              <div class="notification-message">{{ notification.message }}</div>
              <div class="notification-time">{{ notification.time }}</div>
            </div>
          </div>
        </div>
        <mat-divider></mat-divider>
        <div class="menu-footer">
          <button mat-button color="primary">View all notifications</button>
        </div>
      </mat-menu>

      <!-- Profile Menu -->
      <mat-menu #profileMenu="matMenu" class="profile-menu">
        <div class="profile-header">
          <div class="profile-info">
            <div class="profile-name">User Name</div>
            <div class="profile-email">user&#64;dochub.com</div>
          </div>
        </div>
        <mat-divider></mat-divider>
        <button mat-menu-item>
          <mat-icon>person</mat-icon>
          <span>Profile</span>
        </button>
        <button mat-menu-item>
          <mat-icon>settings</mat-icon>
          <span>Settings</span>
        </button>
        <button mat-menu-item>
          <mat-icon>help</mat-icon>
          <span>Help</span>
        </button>
        <mat-divider></mat-divider>
        <button mat-menu-item>
          <mat-icon>logout</mat-icon>
          <span>Sign out</span>
        </button>
      </mat-menu>
    </header>
  `,
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Output() menuToggle = new EventEmitter<void>();

  private themeService = inject(ThemeService);
  isDarkTheme = false;
  searchQuery = '';
  notificationCount = 3;

  notifications = [
    {
      id: 1,
      type: 'success',
      icon: 'check_circle',
      title: 'Letter Generated',
      message: 'Experience letter for John Doe has been generated successfully.',
      time: '2 minutes ago',
      read: false
    },
    {
      id: 2,
      type: 'info',
      icon: 'info',
      title: 'Template Updated',
      message: 'Transfer letter template has been updated by Admin.',
      time: '1 hour ago',
      read: false
    },
    {
      id: 3,
      type: 'warning',
      icon: 'warning',
      title: 'Email Failed',
      message: 'Failed to send confirmation letter to jane@example.com.',
      time: '3 hours ago',
      read: true
    }
  ];

  ngOnInit() {
    this.themeService.isDarkTheme$.subscribe(isDark => {
      this.isDarkTheme = isDark;
    });
  }

  onMenuToggle() {
    this.menuToggle.emit();
  }

  onSearch() {
    if (this.searchQuery.trim()) {
      console.log('Searching for:', this.searchQuery);
      // Implement search functionality
    }
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
