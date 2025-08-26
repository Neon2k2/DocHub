export interface DynamicTabDto {
  id: string;
  name: string;
  displayName: string;
  description: string;
  dataSource: 'Upload' | 'Database';
  icon?: string;
  color?: string;
  sortOrder: number;
  isActive: boolean;
  isAdminOnly: boolean;
  requiredPermission?: string | null;
  databaseQuery?: string | null;
}

export interface CreateDynamicTabDto {
  name: string;
  displayName: string;
  description: string;
  dataSource: string;
  databaseQuery?: string;
  icon?: string;
  color?: string;
  sortOrder: number;
  isAdminOnly: boolean;
  requiredPermission?: string;
}

export interface UpdateDynamicTabDto {
  displayName: string;
  description: string;
  dataSource: string;
  databaseQuery?: string;
  icon?: string;
  color?: string;
  sortOrder: number;
  isActive: boolean;
  isAdminOnly: boolean;
  requiredPermission?: string;
}

export interface TabReorderDto {
  id: string;
  newSortOrder: number;
}
