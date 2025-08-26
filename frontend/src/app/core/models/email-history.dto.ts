export interface EmailHistoryDto {
  id: string;
  subject: string;
  toEmail: string;
  ccEmail?: string;
  bccEmail?: string;
  body: string;
  status: string;
  sentAt?: Date;
  deliveredAt?: Date;
  failedAt?: Date;
  errorMessage?: string;
  emailProvider?: string;
  emailId?: string;
  retryCount: number;
  lastRetryAt?: Date;
  generatedLetterId?: string;
  employeeId?: string;
  employeeName?: string;
  letterType?: string;
  createdAt: Date;
  createdBy: string;
  attachments: EmailAttachmentDto[];
}

export interface CreateEmailHistoryRequest {
  subject: string;
  toEmail: string;
  ccEmail?: string;
  bccEmail?: string;
  body: string;
  generatedLetterId?: string;
  employeeId?: string;
  emailProvider?: string;
  attachments: AddEmailAttachmentRequest[];
}

export interface ResendEmailRequest {
  subject?: string;
  body?: string;
  ccEmail?: string;
  bccEmail?: string;
  useLatestSignature: boolean;
  additionalAttachments: AddEmailAttachmentRequest[];
}

export interface AddEmailAttachmentRequest {
  fileName: string;
  fileType?: string;
  filePath: string;
  fileSize: number;
}

export interface EmailAttachmentDto {
  id: string;
  fileName: string;
  fileType?: string;
  filePath: string;
  fileSize: number;
  createdAt: Date;
}
