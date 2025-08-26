export interface DigitalSignatureDto {
  id: string;
  authorityName: string;
  authorityDesignation: string;
  signatureDate: Date;
  documentHash: string;
  signatureData: string;
  isValid: boolean;
  createdAt: Date;
  isActive: boolean;
}

export interface GenerateSignatureRequest {
  authorityName: string;
  authorityDesignation: string;
  documentHash: string;
  pin?: string;
}

export interface SignDocumentRequest {
  authorityName: string;
  pin: string;
}

export interface ChangePinRequest {
  oldPin: string;
  newPin: string;
}
