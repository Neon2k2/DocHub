import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';

interface EmailHistory {
  id: string;
  subject: string;
  toEmail: string;
  ccEmail?: string;
  bccEmail?: string;
  status: 'sent' | 'delivered' | 'failed' | 'pending';
  sentAt: Date;
  deliveredAt?: Date;
  errorMessage?: string;
  letterType: string;
  employeeName: string;
  attachments: string[];
}

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule]
})
export class HistoryComponent implements OnInit {
  @ViewChild('paginator') paginator: any;

  // Component state
  isLoading = false;
  emailHistory: EmailHistory[] = [];
  
  // Filtering and search
  searchTerm = '';
  statusFilter = 'all';
  dateRange = { start: null, end: null };
  
  // Pagination
  pageSize = 20;
  pageSizeOptions = [10, 20, 50, 100];
  currentPage = 0;
  totalItems = 0;

  // Status options
  statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: 'sent', label: 'Sent' },
    { value: 'delivered', label: 'Delivered' },
    { value: 'failed', label: 'Failed' },
    { value: 'pending', label: 'Pending' }
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadEmailHistory();
  }

  loadEmailHistory(): void {
    this.isLoading = true;
    
    // Mock data for now
    setTimeout(() => {
      this.emailHistory = [
        {
          id: '1',
          subject: 'Transfer Letter - John Doe',
          toEmail: 'john.doe@company.com',
          status: 'delivered',
          sentAt: new Date(Date.now() - 86400000),
          deliveredAt: new Date(Date.now() - 86350000),
          letterType: 'Transfer Letter',
          employeeName: 'John Doe',
          attachments: ['transfer_letter.pdf', 'employee_contract.pdf']
        },
        {
          id: '2',
          subject: 'Experience Certificate - Jane Smith',
          toEmail: 'jane.smith@company.com',
          status: 'sent',
          sentAt: new Date(Date.now() - 172800000),
          letterType: 'Experience Letter',
          employeeName: 'Jane Smith',
          attachments: ['experience_certificate.pdf']
        },
        {
          id: '3',
          subject: 'Confirmation Letter - Mike Johnson',
          toEmail: 'mike.johnson@company.com',
          status: 'failed',
          sentAt: new Date(Date.now() - 259200000),
          errorMessage: 'Invalid email address',
          letterType: 'Confirmation Letter',
          employeeName: 'Mike Johnson',
          attachments: ['confirmation_letter.pdf']
        }
      ];
      
      this.totalItems = this.emailHistory.length;
      this.isLoading = false;
    }, 1000);
  }

  // Filtering methods
  applyFilters(): void {
    // Apply search and status filters
    console.log('Applying filters:', { searchTerm: this.searchTerm, statusFilter: this.statusFilter });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = 'all';
    this.dateRange = { start: null, end: null };
    this.applyFilters();
  }

  // Email actions
  resendEmail(email: EmailHistory): void {
    console.log('Resending email:', email.id);
    // TODO: Implement resend functionality
  }

  viewEmailDetails(email: EmailHistory): void {
    console.log('Viewing email details:', email.id);
    // TODO: Implement email details view
  }

  downloadAttachments(email: EmailHistory): void {
    console.log('Downloading attachments for:', email.id);
    // TODO: Implement attachment download
  }

  exportHistory(): void {
    console.log('Exporting email history');
    // TODO: Implement export functionality
  }

  showBulkActions(): void {
    console.log('Showing bulk actions');
    // TODO: Implement bulk actions dialog
  }

  // Computed properties for template
  get deliveredCount(): number {
    return this.emailHistory.filter(e => e.status === 'delivered').length;
  }

  get sentCount(): number {
    return this.emailHistory.filter(e => e.status === 'sent').length;
  }

  get failedCount(): number {
    return this.emailHistory.filter(e => e.status === 'failed').length;
  }

  get pendingCount(): number {
    return this.emailHistory.filter(e => e.status === 'pending').length;
  }

  // Utility methods
  getStatusColor(status: string): string {
    switch (status) {
      case 'delivered': return 'success';
      case 'sent': return 'primary';
      case 'failed': return 'warn';
      case 'pending': return 'accent';
      default: return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'delivered': return 'check_circle';
      case 'sent': return 'send';
      case 'failed': return 'error';
      case 'pending': return 'schedule';
      default: return 'help';
    }
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInDays = Math.floor(diffInHours / 24);

    if (diffInDays > 0) {
      return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
    } else if (diffInHours > 0) {
      return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }

  // Pagination
  onPageChange(event: any): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    // TODO: Load data for current page
  }
}
