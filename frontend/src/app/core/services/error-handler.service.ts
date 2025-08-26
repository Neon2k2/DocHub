import { Injectable, ErrorHandler, inject } from '@angular/core';
import { NotificationService } from './notification.service';

export interface AppError {
  id: string;
  timestamp: Date;
  message: string;
  stack?: string;
  context?: string;
  userMessage?: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  handled: boolean;
  retryable: boolean;
  retryCount: number;
  maxRetries: number;
}

export interface ApiError {
  status: number;
  statusText: string;
  message: string;
  errors?: string[];
  timestamp: Date;
  endpoint: string;
  method: string;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService implements ErrorHandler {
  private notificationService = inject(NotificationService);
  private errors: AppError[] = [];
  private readonly maxRetries = 3;

  constructor() {
    // Set up global error handler
    window.addEventListener('error', (event) => {
      this.handleError(event.error || new Error(event.message));
    });

    window.addEventListener('unhandledrejection', (event) => {
      this.handleError(new Error(event.reason));
    });
  }

  // Global error handler implementation
  handleError(error: Error | any): void {
    console.error('Error caught by global handler:', error);

    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: error.message || 'Unknown error occurred',
      stack: error.stack,
      context: this.getErrorContext(),
      userMessage: this.getUserFriendlyMessage(error),
      severity: this.determineSeverity(error),
      handled: false,
      retryable: this.isRetryable(error),
      retryCount: 0,
      maxRetries: this.maxRetries
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  // Handle API errors
  handleApiError(apiError: ApiError): void {
    console.error('API Error:', apiError);

    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: apiError.message,
      context: `API: ${apiError.method} ${apiError.endpoint}`,
      userMessage: this.getApiUserMessage(apiError),
      severity: this.determineApiSeverity(apiError.status),
      handled: false,
      retryable: this.isApiRetryable(apiError.status),
      retryCount: 0,
      maxRetries: this.maxRetries
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  // Handle specific error types
  handleValidationError(field: string, message: string): void {
    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: `Validation error in field: ${field}`,
      context: `Validation: ${field}`,
      userMessage: message,
      severity: 'low',
      handled: false,
      retryable: false,
      retryCount: 0,
      maxRetries: 0
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  handleNetworkError(endpoint: string, error: any): void {
    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: `Network error calling ${endpoint}`,
      context: `Network: ${endpoint}`,
      userMessage: 'Unable to connect to the server. Please check your internet connection and try again.',
      severity: 'medium',
      handled: false,
      retryable: true,
      retryCount: 0,
      maxRetries: this.maxRetries
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  handleFileUploadError(fileName: string, error: any): void {
    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: `File upload error for ${fileName}`,
      context: `File Upload: ${fileName}`,
      userMessage: `Failed to upload ${fileName}. Please try again or contact support if the problem persists.`,
      severity: 'medium',
      handled: false,
      retryable: true,
      retryCount: 0,
      maxRetries: this.maxRetries
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  handleSignatureError(operation: string, error: any): void {
    const appError: AppError = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      message: `Digital signature error during ${operation}`,
      context: `Digital Signature: ${operation}`,
      userMessage: `Failed to ${operation} digital signature. Please check your PROXKey device connection and try again.`,
      severity: 'high',
      handled: false,
      retryable: true,
      retryCount: 0,
      maxRetries: this.maxRetries
    };

    this.errors.push(appError);
    this.handleAppError(appError);
  }

  // Retry mechanism
  retryOperation(errorId: string): boolean {
    const error = this.errors.find(e => e.id === errorId);
    if (!error || !error.retryable || error.retryCount >= error.maxRetries) {
      return false;
    }

    error.retryCount++;
    error.handled = false;
    
    // Notify user about retry attempt
    this.notificationService.showInfo(
      'Retrying Operation',
      `Attempt ${error.retryCount} of ${error.maxRetries}...`
    );

    return true;
  }

  // Error recovery
  markErrorAsHandled(errorId: string): void {
    const error = this.errors.find(e => e.id === errorId);
    if (error) {
      error.handled = true;
    }
  }

  // Error reporting
  getUnhandledErrors(): AppError[] {
    return this.errors.filter(e => !e.handled);
  }

  getErrorsBySeverity(severity: AppError['severity']): AppError[] {
    return this.errors.filter(e => e.severity === severity);
  }

  getRecentErrors(hours: number = 24): AppError[] {
    const cutoff = new Date();
    cutoff.setHours(cutoff.getHours() - hours);
    return this.errors.filter(e => e.timestamp > cutoff);
  }

  // Clear old errors
  clearOldErrors(days: number = 7): void {
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - days);
    this.errors = this.errors.filter(e => e.timestamp > cutoff);
  }

  // Export errors for debugging
  exportErrors(): string {
    return JSON.stringify(this.errors, null, 2);
  }

  // Private helper methods
  private handleAppError(error: AppError): void {
    // Log error
    console.error('Application Error:', error);

    // Show user notification based on severity
    switch (error.severity) {
      case 'critical':
        this.notificationService.showError('Critical Error', error.userMessage || error.message, {
          timeOut: 0,
          closeButton: true
        });
        break;
      case 'high':
        this.notificationService.showError('Error', error.userMessage || error.message);
        break;
      case 'medium':
        this.notificationService.showWarning('Warning', error.userMessage || error.message);
        break;
      case 'low':
        this.notificationService.showInfo('Information', error.userMessage || error.message);
        break;
    }

    // Send to monitoring service in production
    if (this.shouldSendToMonitoring(error)) {
      this.sendToMonitoring(error);
    }
  }

  private generateErrorId(): string {
    return `err_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private getErrorContext(): string {
    try {
      const stack = new Error().stack;
      if (stack) {
        const lines = stack.split('\n');
        const callerLine = lines[3] || lines[2] || lines[1];
        return callerLine.trim();
      }
    } catch (e) {
      // Ignore stack trace errors
    }
    return 'Unknown context';
  }

  private getUserFriendlyMessage(error: any): string {
    if (error.userMessage) {
      return error.userMessage;
    }

    if (error.message) {
      // Common error patterns
      if (error.message.includes('Network Error')) {
        return 'Unable to connect to the server. Please check your internet connection.';
      }
      if (error.message.includes('Timeout')) {
        return 'The request timed out. Please try again.';
      }
      if (error.message.includes('Unauthorized')) {
        return 'You are not authorized to perform this action. Please log in again.';
      }
      if (error.message.includes('Forbidden')) {
        return 'Access denied. You do not have permission to perform this action.';
      }
      if (error.message.includes('Not Found')) {
        return 'The requested resource was not found.';
      }
      if (error.message.includes('Internal Server Error')) {
        return 'An internal server error occurred. Please try again later.';
      }
    }

    return 'An unexpected error occurred. Please try again or contact support if the problem persists.';
  }

  private determineSeverity(error: any): AppError['severity'] {
    if (error.critical || error.fatal) {
      return 'critical';
    }
    if (error.severity) {
      return error.severity;
    }
    if (error.name === 'TypeError' || error.name === 'ReferenceError') {
      return 'high';
    }
    return 'medium';
  }

  private determineApiSeverity(status: number): AppError['severity'] {
    if (status >= 500) {
      return 'high';
    }
    if (status >= 400) {
      return 'medium';
    }
    return 'low';
  }

  private isRetryable(error: any): boolean {
    if (error.retryable !== undefined) {
      return error.retryable;
    }
    
    // Network errors are usually retryable
    if (error.message?.includes('Network Error')) {
      return true;
    }
    
    // Timeout errors are retryable
    if (error.message?.includes('Timeout')) {
      return true;
    }
    
    // Server errors (5xx) are retryable
    if (error.status >= 500) {
      return true;
    }
    
    return false;
  }

  private isApiRetryable(status: number): boolean {
    // Retry on server errors and some client errors
    return status >= 500 || status === 408 || status === 429;
  }

  private getApiUserMessage(apiError: ApiError): string {
    switch (apiError.status) {
      case 400:
        return 'Invalid request. Please check your input and try again.';
      case 401:
        return 'Authentication required. Please log in again.';
      case 403:
        return 'Access denied. You do not have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 408:
        return 'Request timeout. Please try again.';
      case 409:
        return 'Conflict. The resource has been modified by another user.';
      case 422:
        return 'Validation error. Please check your input and try again.';
      case 429:
        return 'Too many requests. Please wait a moment and try again.';
      case 500:
        return 'Internal server error. Please try again later.';
      case 502:
        return 'Bad gateway. Please try again later.';
      case 503:
        return 'Service unavailable. Please try again later.';
      case 504:
        return 'Gateway timeout. Please try again later.';
      default:
        return `Request failed with status ${apiError.status}. Please try again.`;
    }
  }

  private shouldSendToMonitoring(error: AppError): boolean {
    // Only send critical and high severity errors to monitoring
    return error.severity === 'critical' || error.severity === 'high';
  }

  private sendToMonitoring(error: AppError): void {
    // In production, this would send to a monitoring service like Sentry, LogRocket, etc.
    console.log('Sending error to monitoring service:', error);
    
    // Example implementation:
    // this.http.post('/api/monitoring/errors', error).subscribe();
  }
}
