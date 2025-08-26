import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PROXKeyInfo {
  deviceId: string;
  serialNumber: string;
  firmwareVersion: string;
  manufacturer: string;
  isInitialized: boolean;
  remainingSignatures: number;
  lastUsed: Date;
}

export interface DigitalSignatureDto {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureImagePath?: string;
  signatureData?: string;
  isActive: boolean;
  sortOrder: number;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface GenerateSignatureRequest {
  authorityName: string;
  authorityDesignation?: string;
  documentHash: string;
}

export interface SignDocumentRequest {
  documentBytes: Uint8Array;
  authorityName: string;
  pin: string;
}

export interface ChangePinRequest {
  oldPin: string;
  newPin: string;
}

@Injectable({
  providedIn: 'root'
})
export class PROXKeyService {
  private readonly apiUrl = `${environment.apiUrl}/api/PROXKey`;
  
  private deviceStatusSubject = new BehaviorSubject<boolean>(false);
  private deviceInfoSubject = new BehaviorSubject<PROXKeyInfo | null>(null);
  
  public deviceStatus$ = this.deviceStatusSubject.asObservable();
  public deviceInfo$ = this.deviceInfoSubject.asObservable();

  constructor(private http: HttpClient) {
    this.checkDeviceStatus();
  }

  /**
   * Check if PROXKey device is connected
   */
  checkDeviceStatus(): Observable<boolean> {
    const observable = this.http.get<{ success: boolean; data: boolean; message: string }>(`${this.apiUrl}/status`);
    
    observable.subscribe({
      next: (response: { success: boolean; data: boolean; message: string }) => {
        this.deviceStatusSubject.next(response.data);
        if (response.data) {
          this.getDeviceInfo();
        }
      },
      error: (error) => {
        console.error('Error checking PROXKey device status:', error);
        this.deviceStatusSubject.next(false);
      }
    });
    
    return observable.pipe(
      map((response: { success: boolean; data: boolean; message: string }) => response.data)
    );
  }

  /**
   * Get detailed information about the PROXKey device
   */
  getDeviceInfo(): Observable<PROXKeyInfo> {
    const observable = this.http.get<{ success: boolean; data: PROXKeyInfo; message: string }>(`${this.apiUrl}/info`);
    
    observable.subscribe({
      next: (response: { success: boolean; data: PROXKeyInfo; message: string }) => {
        this.deviceInfoSubject.next(response.data);
      },
      error: (error) => {
        console.error('Error getting PROXKey device info:', error);
        this.deviceInfoSubject.next(null);
      }
    });
    
    return observable.pipe(
      map((response: { success: boolean; data: PROXKeyInfo; message: string }) => response.data)
    );
  }

  /**
   * Generate a digital signature using PROXKey device
   */
  generateSignature(request: GenerateSignatureRequest): Observable<DigitalSignatureDto> {
    return this.http.post<{ success: boolean; data: DigitalSignatureDto; message: string }>(
      `${this.apiUrl}/generate-signature`, 
      request
    ).pipe(
      map((response: { success: boolean; data: DigitalSignatureDto; message: string }) => response.data)
    );
  }

  /**
   * Sign a document using PROXKey device
   */
  signDocument(request: SignDocumentRequest): Observable<Uint8Array> {
    // Convert Uint8Array to base64 for transmission
    const base64Document = btoa(String.fromCharCode(...request.documentBytes));
    
    const signRequest = {
      documentBytes: base64Document,
      authorityName: request.authorityName,
      pin: request.pin
    };

    return this.http.post<{ success: boolean; data: string; message: string }>(
      `${this.apiUrl}/sign-document`, 
      signRequest
    ).pipe(
      map((response: { success: boolean; data: string; message: string }) => {
        // Convert base64 response back to Uint8Array
        const binaryString = atob(response.data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
          bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes;
      })
    );
  }

  /**
   * Validate a digital signature
   */
  validateSignature(documentBytes: Uint8Array, signature: Uint8Array): Observable<boolean> {
    const base64Document = btoa(String.fromCharCode(...documentBytes));
    const base64Signature = btoa(String.fromCharCode(...signature));
    
    const request = {
      documentBytes: base64Document,
      signature: base64Signature
    };

    return this.http.post<{ success: boolean; data: boolean; message: string }>(
      `${this.apiUrl}/validate-signature`, 
      request
    ).pipe(
      map((response: { success: boolean; data: boolean; message: string }) => response.data)
    );
  }

  /**
   * Get device status information
   */
  getDeviceStatusInfo(): Observable<string> {
    return this.http.get<{ success: boolean; data: string; message: string }>(
      `${this.apiUrl}/device-status`
    ).pipe(
      map((response: { success: boolean; data: string; message: string }) => response.data)
    );
  }

  /**
   * Change PROXKey device PIN
   */
  changePin(request: ChangePinRequest): Observable<boolean> {
    return this.http.post<{ success: boolean; data: boolean; message: string }>(
      `${this.apiUrl}/change-pin`, 
      request
    ).pipe(
      map((response: { success: boolean; data: boolean; message: string }) => response.data)
    );
  }

  /**
   * Reset PROXKey device (use with caution)
   */
  resetDevice(): Observable<boolean> {
    return this.http.post<{ success: boolean; data: boolean; message: string }>(
      `${this.apiUrl}/reset-device`,
      {} // Add empty body
    ).pipe(
      map((response: { success: boolean; data: boolean; message: string }) => response.data)
    );
  }

  /**
   * Generate document hash for signing
   */
  async generateDocumentHash(documentBytes: Uint8Array): Promise<string> {
    try {
      const hashBuffer = await crypto.subtle.digest('SHA-256', documentBytes);
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
      return hashHex;
    } catch (error) {
      console.error('Error generating document hash:', error);
      throw new Error('Failed to generate document hash');
    }
  }

  /**
   * Convert file to Uint8Array
   */
  async fileToUint8Array(file: File): Promise<Uint8Array> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        if (reader.result instanceof ArrayBuffer) {
          resolve(new Uint8Array(reader.result));
        } else {
          reject(new Error('Failed to read file'));
        }
      };
      reader.onerror = () => reject(new Error('Failed to read file'));
      reader.readAsArrayBuffer(file);
    });
  }

  /**
   * Download signed document
   */
  downloadSignedDocument(documentBytes: Uint8Array, filename: string): void {
    const blob = new Blob([documentBytes], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Get current device status
   */
  getCurrentDeviceStatus(): boolean {
    return this.deviceStatusSubject.value;
  }

  /**
   * Get current device info
   */
  getCurrentDeviceInfo(): PROXKeyInfo | null {
    return this.deviceInfoSubject.value;
  }

  /**
   * Refresh device status and info
   */
  refreshDeviceInfo(): void {
    this.checkDeviceStatus();
  }
}


