import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EmailStatusUpdate {
  emailId: string;
  status: string;
  message: string;
  timestamp: Date;
  errorDetails?: string;
  retryCount: number;
  nextRetryTime?: Date;
}

export interface BulkEmailProgress {
  current: number;
  total: number;
  successful: number;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private connectionEstablished = new BehaviorSubject<boolean>(false);
  private emailStatusUpdates = new Subject<EmailStatusUpdate>();
  private bulkEmailStarted = new Subject<number>();
  private bulkEmailProgress = new Subject<BulkEmailProgress>();
  private bulkEmailCompleted = new Subject<{ total: number; successful: number }>();

  public connectionEstablished$ = this.connectionEstablished.asObservable();
  public emailStatusUpdates$ = this.emailStatusUpdates.asObservable();
  public bulkEmailStarted$ = this.bulkEmailStarted.asObservable();
  public bulkEmailProgress$ = this.bulkEmailProgress.asObservable();
  public bulkEmailCompleted$ = this.bulkEmailCompleted.asObservable();

  constructor() {
    this.initializeConnection();
  }

  private async initializeConnection() {
    try {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.apiUrl}/emailStatusHub`)
        .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry pattern
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      this.setupEventHandlers();

      // Start connection
      await this.hubConnection.start();
      console.log('SignalR connection established');
      this.connectionEstablished.next(true);

      // Handle reconnection
      this.hubConnection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        this.connectionEstablished.next(false);
      });

      this.hubConnection.onreconnected(() => {
        console.log('SignalR reconnected');
        this.connectionEstablished.next(true);
      });

      this.hubConnection.onclose(() => {
        console.log('SignalR connection closed');
        this.connectionEstablished.next(false);
      });

    } catch (error) {
      console.error('Error establishing SignalR connection:', error);
      this.connectionEstablished.next(false);
    }
  }

  private setupEventHandlers() {
    if (!this.hubConnection) return;

    // Email status updates
    this.hubConnection.on('EmailStatusUpdated', (emailId: string, status: EmailStatusUpdate) => {
      console.log('Email status update received:', emailId, status);
      this.emailStatusUpdates.next(status);
    });

    // Bulk email events
    this.hubConnection.on('BulkEmailStarted', (totalEmails: number) => {
      console.log('Bulk email started:', totalEmails);
      this.bulkEmailStarted.next(totalEmails);
    });

    this.hubConnection.on('BulkEmailProgress', (current: number, total: number, successful: number) => {
      console.log('Bulk email progress:', current, total, successful);
      this.bulkEmailProgress.next({ current, total, successful });
    });

    this.hubConnection.on('BulkEmailCompleted', (total: number, successful: number) => {
      console.log('Bulk email completed:', total, successful);
      this.bulkEmailCompleted.next({ total, successful });
    });
  }

  // Join a specific email's status group
  async joinEmailGroup(emailId: string): Promise<void> {
    if (this.hubConnection && this.connectionEstablished.value) {
      try {
        await this.hubConnection.invoke('JoinEmailGroup', emailId);
        console.log(`Joined email group: ${emailId}`);
      } catch (error) {
        console.error('Error joining email group:', error);
      }
    }
  }

  // Leave a specific email's status group
  async leaveEmailGroup(emailId: string): Promise<void> {
    if (this.hubConnection && this.connectionEstablished.value) {
      try {
        await this.hubConnection.invoke('LeaveEmailGroup', emailId);
        console.log(`Left email group: ${emailId}`);
      } catch (error) {
        console.error('Error leaving email group:', error);
      }
    }
  }

  // Get connection status
  isConnected(): boolean {
    return this.connectionEstablished.value;
  }

  // Manually reconnect
  async reconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        await this.initializeConnection();
      } catch (error) {
        console.error('Error reconnecting:', error);
      }
    }
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
