import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface LetterTemplate {
  id: string;
  name: string;
  displayName: string;
  description: string;
  letterType?: string;
  isActive: boolean;
  sortOrder: number;
  useUploadedData: boolean;
  dataSource?: string;
  fields: LetterTemplateField[];
  createdAt: Date;
  updatedAt?: Date;
}

export interface LetterTemplateField {
  id: string;
  fieldName: string;
  displayName: string;
  placeholder: string;
  dataType: string;
  isRequired: boolean;
  defaultValue?: string;
  sortOrder: number;
}

export interface Employee {
  id: string;
  employeeId: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  email: string;
  phoneNumber?: string;
  department?: string;
  designation?: string;
  joiningDate?: Date;
  terminationDate?: Date;
  isActive: boolean;
  dataSource?: string;
  createdAt: Date;
}

export interface DigitalSignature {
  id: string;
  signatureName: string;
  authorityName: string;
  authorityDesignation: string;
  signatureImagePath: string;
  signatureImageData?: string;
  signatureDate: Date;
  isActive: boolean;
  sortOrder: number;
  createdAt: Date;
}

export interface GeneratedLetter {
  id: string;
  letterNumber: string;
  subject: string;
  content?: string;
  generatedFilePath: string;
  emailBody?: string;
  emailSubject?: string;
  sentAt?: Date;
  sentTo?: string;
  sentBy?: string;
  emailStatus?: string;
  emailMessageId?: string;
  errorMessage?: string;
  retryCount: number;
  lastRetryAt?: Date;
  createdAt: Date;
  createdBy: string;
  letterTemplate: LetterTemplate;
  employee: Employee;
  digitalSignature: DigitalSignature;
  attachments: LetterAttachment[];
}

export interface LetterAttachment {
  id: string;
  fileName: string;
  fileType: string;
  filePath: string;
  fileSize: number;
  description?: string;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private _templates = new BehaviorSubject<LetterTemplate[]>([]);
  private _employees = new BehaviorSubject<Employee[]>([]);
  private _signatures = new BehaviorSubject<DigitalSignature[]>([]);
  private _generatedLetters = new BehaviorSubject<GeneratedLetter[]>([]);
  private _isLoading = new BehaviorSubject<boolean>(false);

  public templates$ = this._templates.asObservable();
  public employees$ = this._employees.asObservable();
  public signatures$ = this._signatures.asObservable();
  public generatedLetters$ = this._generatedLetters.asObservable();
  public isLoading$ = this._isLoading.asObservable();

  constructor(private apiService: ApiService) {}

  // Letter Templates
  loadTemplates(): Observable<LetterTemplate[]> {
    this._isLoading.next(true);
    
    this.apiService.getLetterTemplates().subscribe({
      next: (response) => {
        if (response.success) {
          this._templates.next(response.data);
        }
        this._isLoading.next(false);
      },
      error: (error) => {
        console.error('Error loading templates:', error);
        this._isLoading.next(false);
      }
    });

    return this.templates$;
  }

  getTemplateById(id: string): LetterTemplate | undefined {
    return this._templates.value.find(t => t.id === id);
  }

  getActiveTemplates(): LetterTemplate[] {
    return this._templates.value.filter(t => t.isActive);
  }

  createTemplate(template: Partial<LetterTemplate>): Observable<any> {
    return this.apiService.createLetterTemplate(template);
  }

  updateTemplate(id: string, template: Partial<LetterTemplate>): Observable<any> {
    return this.apiService.updateLetterTemplate(id, template);
  }

  deleteTemplate(id: string): Observable<any> {
    return this.apiService.deleteLetterTemplate(id);
  }

  // Employees
  loadEmployees(): Observable<Employee[]> {
    this._isLoading.next(true);
    
    this.apiService.getEmployees().subscribe({
      next: (response) => {
        if (response.success) {
          this._employees.next(response.data.data);
        }
        this._isLoading.next(false);
      },
      error: (error) => {
        console.error('Error loading employees:', error);
        this._isLoading.next(false);
      }
    });

    return this.employees$;
  }

  getEmployeeById(id: string): Employee | undefined {
    return this._employees.value.find(e => e.id === id);
  }

  getActiveEmployees(): Employee[] {
    return this._employees.value.filter(e => e.isActive);
  }

  searchEmployees(query: string): Employee[] {
    if (!query) return this._employees.value;
    
    const lowerQuery = query.toLowerCase();
    return this._employees.value.filter(e => 
      e.firstName.toLowerCase().includes(lowerQuery) ||
      e.lastName.toLowerCase().includes(lowerQuery) ||
      e.employeeId.toLowerCase().includes(lowerQuery) ||
      e.email.toLowerCase().includes(lowerQuery) ||
      (e.department && e.department.toLowerCase().includes(lowerQuery)) ||
      (e.designation && e.designation.toLowerCase().includes(lowerQuery))
    );
  }

  createEmployee(employee: Partial<Employee>): Observable<any> {
    return this.apiService.createEmployee(employee);
  }

  updateEmployee(id: string, employee: Partial<Employee>): Observable<any> {
    return this.apiService.updateEmployee(id, employee);
  }

  deleteEmployee(id: string): Observable<any> {
    return this.apiService.deleteEmployee(id);
  }

  // Digital Signatures
  loadSignatures(): Observable<DigitalSignature[]> {
    this._isLoading.next(true);
    
    this.apiService.getDigitalSignatures().subscribe({
      next: (response) => {
        if (response.success) {
          this._signatures.next(response.data);
        }
        this._isLoading.next(false);
      },
      error: (error) => {
        console.error('Error loading signatures:', error);
        this._isLoading.next(false);
      }
    });

    return this.signatures$;
  }

  getSignatureById(id: string): DigitalSignature | undefined {
    return this._signatures.value.find(s => s.id === id);
  }

  getActiveSignatures(): DigitalSignature[] {
    return this._signatures.value.filter(s => s.isActive);
  }

  createSignature(signature: Partial<DigitalSignature>): Observable<any> {
    return this.apiService.createDigitalSignature(signature);
  }

  updateSignature(id: string, signature: Partial<DigitalSignature>): Observable<any> {
    return this.apiService.updateDigitalSignature(id, signature);
  }

  deleteSignature(id: string): Observable<any> {
    return this.apiService.deleteDigitalSignature(id);
  }

  // Generated Letters
  loadGeneratedLetters(): Observable<GeneratedLetter[]> {
    this._isLoading.next(true);
    
    this.apiService.getGeneratedLetters().subscribe({
      next: (response) => {
        if (response.success) {
          this._generatedLetters.next(response.data.data);
        }
        this._isLoading.next(false);
      },
      error: (error) => {
        console.error('Error loading generated letters:', error);
        this._isLoading.next(false);
      }
    });

    return this.generatedLetters$;
  }

  getGeneratedLetterById(id: string): GeneratedLetter | undefined {
    return this._generatedLetters.value.find(l => l.id === id);
  }

  getLettersByStatus(status: string): GeneratedLetter[] {
    return this._generatedLetters.value.filter(l => l.emailStatus === status);
  }

  generateLetter(request: any): Observable<any> {
    return this.apiService.generateLetter(request);
  }

  sendEmail(request: any): Observable<any> {
    return this.apiService.sendEmail(request);
  }

  resendEmail(id: string): Observable<any> {
    return this.apiService.resendEmail(id);
  }

  // File Upload
  uploadFile(file: File, type: string): Observable<any> {
    return this.apiService.uploadFile(file, type);
  }

  uploadExcelData(file: File): Observable<any> {
    return this.apiService.uploadExcelData(file);
  }

  // Utility methods
  refreshAllData(): void {
    this.loadTemplates();
    this.loadEmployees();
    this.loadSignatures();
    this.loadGeneratedLetters();
  }

  getDashboardStats() {
    const templates = this._templates.value;
    const employees = this._employees.value;
    const letters = this._generatedLetters.value;
    const signatures = this._signatures.value;

    return {
      totalTemplates: templates.length,
      activeTemplates: templates.filter(t => t.isActive).length,
      totalEmployees: employees.length,
      activeEmployees: employees.filter(e => e.isActive).length,
      totalLetters: letters.length,
      pendingEmails: letters.filter(l => l.emailStatus === 'pending').length,
      totalSignatures: signatures.length,
      activeSignatures: signatures.filter(s => s.isActive).length
    };
  }
}
