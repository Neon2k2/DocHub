import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  read: boolean;
  action?: {
    label: string;
    callback: () => void;
  };
  autoClose?: boolean;
  duration?: number;
}

export interface ToastNotification {
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  options?: {
    timeOut?: number;
    closeButton?: boolean;
    progressBar?: boolean;
    enableHtml?: boolean;
    positionClass?: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications = new BehaviorSubject<Notification[]>([]);
  private unreadCount = new BehaviorSubject<number>(0);

  constructor(private toastr: ToastrService) {
    this.loadNotificationsFromStorage();
  }

  // Observable streams
  get notifications$(): Observable<Notification[]> {
    return this.notifications.asObservable();
  }

  get unreadCount$(): Observable<number> {
    return this.unreadCount.asObservable();
  }

  // Toast notifications
  showSuccess(title: string, message: string, options?: any): void {
    this.showToast({ type: 'success', title, message, options });
  }

  showError(title: string, message: string, options?: any): void {
    this.showToast({ type: 'error', title, message, options });
  }

  showWarning(title: string, message: string, options?: any): void {
    this.showToast({ type: 'warning', title, message, options });
  }

  showInfo(title: string, message: string, options?: any): void {
    this.showToast({ type: 'info', title, message, options });
  }

  private showToast(toast: ToastNotification): void {
    const defaultOptions = {
      timeOut: 5000,
      closeButton: true,
      progressBar: true,
      enableHtml: true,
      positionClass: 'toast-top-right'
    };

    const finalOptions = { ...defaultOptions, ...toast.options };

    switch (toast.type) {
      case 'success':
        this.toastr.success(toast.message, toast.title, finalOptions);
        break;
      case 'error':
        this.toastr.error(toast.message, toast.title, finalOptions);
        break;
      case 'warning':
        this.toastr.warning(toast.message, toast.title, finalOptions);
        break;
      case 'info':
        this.toastr.info(toast.message, toast.title, finalOptions);
        break;
    }
  }

  // In-app notifications
  addNotification(notification: Omit<Notification, 'id' | 'timestamp' | 'read'>): string {
    const newNotification: Notification = {
      ...notification,
      id: this.generateId(),
      timestamp: new Date(),
      read: false
    };

    const currentNotifications = this.notifications.value;
    const updatedNotifications = [newNotification, ...currentNotifications];
    
    this.notifications.next(updatedNotifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
    
    return newNotification.id;
  }

  markAsRead(id: string): void {
    const currentNotifications = this.notifications.value;
    const updatedNotifications = currentNotifications.map(notification =>
      notification.id === id ? { ...notification, read: true } : notification
    );
    
    this.notifications.next(updatedNotifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  markAllAsRead(): void {
    const currentNotifications = this.notifications.value;
    const updatedNotifications = currentNotifications.map(notification =>
      ({ ...notification, read: true })
    );
    
    this.notifications.next(updatedNotifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  removeNotification(id: string): void {
    const currentNotifications = this.notifications.value;
    const updatedNotifications = currentNotifications.filter(notification => notification.id !== id);
    
    this.notifications.next(updatedNotifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  clearAllNotifications(): void {
    this.notifications.next([]);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  // Specific notification types for common actions
  notifyLetterGenerated(employeeName: string, letterType: string): string {
    return this.addNotification({
      type: 'success',
      title: 'Letter Generated Successfully',
      message: `${letterType} letter for ${employeeName} has been generated and is ready for review.`,
      action: {
        label: 'View Letter',
        callback: () => {
          // Navigate to letter preview
          console.log('Navigate to letter preview');
        }
      }
    });
  }

  notifyEmailSent(employeeName: string, email: string, letterType: string): string {
    return this.addNotification({
      type: 'success',
      title: 'Email Sent Successfully',
      message: `${letterType} letter has been sent to ${employeeName} at ${email}.`,
      action: {
        label: 'View Status',
        callback: () => {
          // Navigate to email history
          console.log('Navigate to email history');
        }
      }
    });
  }

  notifyEmailFailed(employeeName: string, error: string): string {
    return this.addNotification({
      type: 'error',
      title: 'Email Failed to Send',
      message: `Failed to send letter to ${employeeName}. Error: ${error}`,
      action: {
        label: 'Retry',
        callback: () => {
          // Retry sending email
          console.log('Retry sending email');
        }
      }
    });
  }

  notifySignatureGenerated(authorityName: string): string {
    return this.addNotification({
      type: 'success',
      title: 'Digital Signature Generated',
      message: `Digital signature for ${authorityName} has been generated successfully using PROXKey device.`,
      action: {
        label: 'View Signature',
        callback: () => {
          // Navigate to signature management
          console.log('Navigate to signature management');
        }
      }
    });
  }

  notifyFileUploaded(fileName: string, fileType: string): string {
    return this.addNotification({
      type: 'success',
      title: 'File Uploaded Successfully',
      message: `${fileName} (${fileType}) has been uploaded and processed.`,
      action: {
        label: 'View File',
        callback: () => {
          // Navigate to file management
          console.log('Navigate to file management');
        }
      }
    });
  }

  notifyTemplateUpdated(templateName: string): string {
    return this.addNotification({
      type: 'info',
      title: 'Template Updated',
      message: `${templateName} template has been updated and is now available for use.`,
      action: {
        label: 'View Template',
        callback: () => {
          // Navigate to template management
          console.log('Navigate to template management');
        }
      }
    });
  }

  notifyBulkOperationStarted(operation: string, count: number): string {
    return this.addNotification({
      type: 'info',
      title: 'Bulk Operation Started',
      message: `${operation} operation has started for ${count} items. You will be notified when complete.`,
      autoClose: false
    });
  }

  notifyBulkOperationCompleted(operation: string, successCount: number, totalCount: number): string {
    const type = successCount === totalCount ? 'success' : 'warning';
    const message = successCount === totalCount 
      ? `${operation} completed successfully for all ${totalCount} items.`
      : `${operation} completed with ${successCount} successful out of ${totalCount} items.`;

    return this.addNotification({
      type,
      title: 'Bulk Operation Completed',
      message,
      action: {
        label: 'View Results',
        callback: () => {
          // Navigate to results view
          console.log('Navigate to results view');
        }
      }
    });
  }

  // Real-time status updates
  updateEmailStatus(emailId: string, status: string, details?: any): void {
    const currentNotifications = this.notifications.value;
    const existingNotification = currentNotifications.find(n => 
      n.message.includes(emailId) && n.title.includes('Email')
    );

    if (existingNotification) {
      const updatedNotification = {
        ...existingNotification,
        message: `Email status updated: ${status}${details ? ` - ${details}` : ''}`,
        timestamp: new Date()
      };

      const updatedNotifications = currentNotifications.map(n =>
        n.id === existingNotification.id ? updatedNotification : n
      );

      this.notifications.next(updatedNotifications);
      this.saveNotificationsToStorage();
    }
  }

  // Utility methods
  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  private updateUnreadCount(): void {
    const unreadCount = this.notifications.value.filter(n => !n.read).length;
    this.unreadCount.next(unreadCount);
  }

  private saveNotificationsToStorage(): void {
    try {
      localStorage.setItem('dochub_notifications', JSON.stringify(this.notifications.value));
    } catch (error) {
      console.warn('Failed to save notifications to localStorage:', error);
    }
  }

  private loadNotificationsFromStorage(): void {
    try {
      const stored = localStorage.getItem('dochub_notifications');
      if (stored) {
        const parsedNotifications = JSON.parse(stored).map((n: any) => ({
          ...n,
          timestamp: new Date(n.timestamp)
        }));
        this.notifications.next(parsedNotifications);
        this.updateUnreadCount();
      }
    } catch (error) {
      console.warn('Failed to load notifications from localStorage:', error);
    }
  }

  // Auto-cleanup old notifications (older than 30 days)
  cleanupOldNotifications(): void {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

    const currentNotifications = this.notifications.value;
    const filteredNotifications = currentNotifications.filter(notification =>
      notification.timestamp > thirtyDaysAgo
    );

    if (filteredNotifications.length !== currentNotifications.length) {
      this.notifications.next(filteredNotifications);
      this.updateUnreadCount();
      this.saveNotificationsToStorage();
    }
  }

  // Get notifications by type
  getNotificationsByType(type: Notification['type']): Notification[] {
    return this.notifications.value.filter(n => n.type === type);
  }

  // Get recent notifications (last 24 hours)
  getRecentNotifications(): Notification[] {
    const twentyFourHoursAgo = new Date();
    twentyFourHoursAgo.setHours(twentyFourHoursAgo.getHours() - 24);
    
    return this.notifications.value.filter(n => n.timestamp > twentyFourHoursAgo);
  }
}
