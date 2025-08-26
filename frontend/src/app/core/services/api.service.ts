import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Generic HTTP methods
  get<T>(endpoint: string, params?: HttpParams): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, { params });
  }

  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, data);
  }

  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, data);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`);
  }

  // Dashboard & Statistics
  getDashboardStats(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/Dashboard/stats');
  }

  getDashboardRecentActivity(): Observable<ApiResponse<any[]>> {
    return this.get<ApiResponse<any[]>>('/api/Dashboard/recent-activity');
  }

  // Letter Templates
  getLetterTemplates(): Observable<ApiResponse<any[]>> {
    return this.get<ApiResponse<any[]>>('/api/LetterTemplates');
  }

  getLetterTemplate(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/LetterTemplates/${id}`);
  }

  createLetterTemplate(template: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/LetterTemplates', template);
  }

  updateLetterTemplate(id: string, template: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/LetterTemplates/${id}`, template);
  }

  deleteLetterTemplate(id: string): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`/api/LetterTemplates/${id}`);
  }

  toggleTemplateActive(id: string): Observable<ApiResponse<boolean>> {
    return this.post<ApiResponse<boolean>>(`/api/LetterTemplates/${id}/toggle-active`, {});
  }

  // Employees
  getEmployees(params?: HttpParams): Observable<ApiResponse<PaginatedResponse<any>>> {
    return this.get<ApiResponse<PaginatedResponse<any>>>('/api/Employees', params);
  }

  getEmployee(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/Employees/${id}`);
  }

  createEmployee(employee: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/Employees', employee);
  }

  updateEmployee(id: string, employee: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/Employees/${id}`, employee);
  }

  deleteEmployee(id: string): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`/api/Employees/${id}`);
  }

  bulkUpdateEmployees(employees: any[]): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/Employees/bulk-update', { employees });
  }

  // Digital Signatures
  getDigitalSignatures(): Observable<ApiResponse<any[]>> {
    return this.get<ApiResponse<any[]>>('/api/DigitalSignatures');
  }

  getDigitalSignature(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/DigitalSignatures/${id}`);
  }

  createDigitalSignature(signature: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/DigitalSignatures', signature);
  }

  updateDigitalSignature(id: string, signature: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/DigitalSignatures/${id}`, signature);
  }

  deleteDigitalSignature(id: string): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`/api/DigitalSignatures/${id}`);
  }

  getLatestSignature(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/DigitalSignatures/latest');
  }

  testDeviceConnection(): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/DigitalSignatures/test-connection', {});
  }

  getSignatureStats(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/DigitalSignatures/stats');
  }

  // Generated Letters
  getGeneratedLetters(params?: HttpParams): Observable<ApiResponse<PaginatedResponse<any>>> {
    return this.get<ApiResponse<PaginatedResponse<any>>>('/api/GeneratedLetters', params);
  }

  getGeneratedLetter(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/GeneratedLetters/${id}`);
  }

  createGeneratedLetter(letter: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/GeneratedLetters', letter);
  }

  updateGeneratedLetter(id: string, letter: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/GeneratedLetters/${id}`, letter);
  }

  deleteGeneratedLetter(id: string): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`/api/GeneratedLetters/${id}`);
  }

  // Letter Generation
  generateLetter(request: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/LetterGeneration/generate', request);
  }

  generateBulkLetters(request: any): Observable<ApiResponse<any[]>> {
    return this.post<ApiResponse<any[]>>('/api/LetterGeneration/generate-bulk', request);
  }

  previewLetter(request: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/LetterGeneration/preview', request);
  }

  // Email Operations
  sendEmail(request: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/LetterGeneration/send-email', request);
  }

  sendBulkEmails(request: any): Observable<ApiResponse<any[]>> {
    return this.post<ApiResponse<any[]>>('/api/LetterGeneration/send-bulk-emails', request);
  }

  resendEmail(id: string): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>(`/api/LetterGeneration/resend-email/${id}`, {});
  }

  getEmailStatus(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/LetterGeneration/email-status/${id}`);
  }

  updateEmailStatus(id: string, status: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/LetterGeneration/email-status/${id}`, status);
  }

  // File Upload
  uploadFile(file: File, type: string): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('type', type);

    return this.post<ApiResponse<any>>('/api/FileUpload/upload', formData);
  }

  uploadExcelData(file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.post<ApiResponse<any>>('/api/FileUpload/excel', formData);
  }

  uploadTemplate(file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.post<ApiResponse<any>>('/api/FileUpload/template', formData);
  }

  uploadSignature(file: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.post<ApiResponse<any>>('/api/FileUpload/signature', formData);
  }

  getUploadHistory(params?: HttpParams): Observable<ApiResponse<PaginatedResponse<any>>> {
    return this.get<ApiResponse<PaginatedResponse<any>>>('/api/FileUpload/history', params);
  }

  deleteUploadedFile(id: string): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`/api/FileUpload/${id}`);
  }

  // PROXKey Operations
  getDeviceInfo(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/DigitalSignature/device-info');
  }

  generateSignatureWithDevice(request: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/DigitalSignature/generate', request);
  }

  validateSignature(signature: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/DigitalSignature/validate', signature);
  }

  getSignatureImage(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/api/DigitalSignature/image/${id}`, { responseType: 'blob' });
  }

  // Data Source Management
  getDataSources(): Observable<ApiResponse<any[]>> {
    return this.get<ApiResponse<any[]>>('/api/LetterTemplates');
  }

  toggleDataSource(id: string, source: 'upload' | 'database'): Observable<ApiResponse<any>> {
    const dataSource = source === 'upload' ? 'Upload' : 'Database';
    return this.put<ApiResponse<any>>(`/api/LetterTemplates/${id}/datasource`, { dataSource });
  }

  getDataSourceFields(id: string): Observable<ApiResponse<any[]>> {
    return this.get<ApiResponse<any[]>>(`/api/LetterTemplates/${id}/fields`);
  }

  // Webhook & Notifications
  getWebhookStatus(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/Webhook/verify');
  }

  // Search & Filtering
  searchEmployees(query: string): Observable<ApiResponse<any[]>> {
    const params = new HttpParams().set('q', query);
    return this.get<ApiResponse<any[]>>('/api/Employees/search', params);
  }

  searchLetters(query: string): Observable<ApiResponse<any[]>> {
    const params = new HttpParams().set('q', query);
    return this.get<ApiResponse<any[]>>('/api/GeneratedLetters/search', params);
  }

  // Export & Reports
  exportEmployees(format: 'excel' | 'pdf' | 'csv'): Observable<Blob> {
    const params = new HttpParams().set('format', format);
    return this.http.get(`${this.baseUrl}/api/Employees/export`, { 
      params, 
      responseType: 'blob' 
    });
  }

  exportLetterHistory(format: 'excel' | 'pdf' | 'csv'): Observable<Blob> {
    const params = new HttpParams().set('format', format);
    return this.http.get(`${this.baseUrl}/api/GeneratedLetters/export`, { 
      params, 
      responseType: 'blob' 
    });
  }

  // Digital Signature Operations
  downloadSignature(id: string): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>(`/api/DigitalSignature/${id}/download`);
  }

  renewSignature(id: string): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/DigitalSignature/${id}/renew`, {});
  }

  deactivateSignature(id: string): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>(`/api/DigitalSignature/${id}/deactivate`, {});
  }

  deleteSignature(id: string): Observable<ApiResponse<any>> {
    return this.delete<ApiResponse<any>>(`/api/DigitalSignature/${id}`);
  }

  bulkDeactivateSignatures(ids: string[]): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>('/api/DigitalSignature/bulk-deactivate', { ids });
  }

  bulkDeleteSignatures(ids: string[]): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>('/api/DigitalSignature/bulk-delete', { ids });
  }

  exportSignatures(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/api/DigitalSignature/export`, { 
      responseType: 'blob' 
    });
  }

  // Settings & Configuration
  getApplicationSettings(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/Settings');
  }

  updateApplicationSettings(settings: any): Observable<ApiResponse<any>> {
    return this.put<ApiResponse<any>>('/api/Settings', settings);
  }

  resetApplicationSettings(): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/Settings/reset', {});
  }

  exportApplicationSettings(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/api/Settings/export`, { 
      responseType: 'blob' 
    });
  }

  testEmailSettings(emailSettings: any): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/Settings/test-email', emailSettings);
  }

  testProXKeyConnection(endpoint: string): Observable<ApiResponse<any>> {
    return this.post<ApiResponse<any>>('/api/Settings/test-proxkey', { endpoint });
  }

  // Health Check
  getHealthStatus(): Observable<ApiResponse<any>> {
    return this.get<ApiResponse<any>>('/api/health');
  }
}
