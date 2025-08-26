import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { DynamicTabDto } from '../models/dynamic-tab.dto';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class DynamicTabService {

  constructor(private apiService: ApiService) { }

  getAllActiveTabs(): Observable<DynamicTabDto[]> {
    return this.apiService.get<DynamicTabDto[]>('/api/DynamicTab').pipe(
      map(response => response),
      catchError(error => {
        console.error('Error fetching dynamic tabs:', error);
        // Fallback to mock data if API is not available
        return of(this.getMockTabs());
      })
    );
  }

  private getMockTabs(): DynamicTabDto[] {
    return [
      {
        id: '1',
        name: 'transfer-letter',
        displayName: 'Transfer Letter',
        description: 'Employee transfer notifications',
        dataSource: 'Upload',
        icon: 'transfer',
        color: '#3B82F6',
        sortOrder: 1,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      },
      {
        id: '2',
        name: 'experience-letter',
        displayName: 'Experience Letter',
        description: 'Employee experience certificates',
        dataSource: 'Upload',
        icon: 'experience',
        color: '#10B981',
        sortOrder: 2,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      },
      {
        id: '3',
        name: 'confirmation-letter',
        displayName: 'Confirmation Letter',
        description: 'Employee confirmation letters',
        dataSource: 'Upload',
        icon: 'confirmation',
        color: '#F59E0B',
        sortOrder: 3,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      },
      {
        id: '4',
        name: 'mutual-cessation-letter',
        displayName: 'Mutual Cessation Letter',
        description: 'Mutual termination agreements',
        dataSource: 'Upload',
        icon: 'cessation',
        color: '#EF4444',
        sortOrder: 4,
        isActive: true,
        isAdminOnly: false,
        requiredPermission: null,
        databaseQuery: null
      }
    ];
  }

  getDynamicTabs(): Observable<{ success: boolean; data?: DynamicTabDto[]; message?: string }> {
    return this.apiService.get<DynamicTabDto[]>('/api/DynamicTab').pipe(
      map(response => ({ success: true, data: response })),
      catchError(error => {
        console.error('Error fetching dynamic tabs:', error);
        return of({ success: false, message: 'Failed to fetch dynamic tabs' });
      })
    );
  }

  getTabById(id: string): Observable<DynamicTabDto | null> {
    return this.apiService.get<DynamicTabDto>(`/api/DynamicTab/${id}`).pipe(
      map(response => response),
      catchError(error => {
        console.error(`Error fetching tab by id ${id}:`, error);
        return this.getAllActiveTabs().pipe(
          map(tabs => tabs.find(tab => tab.id === id) || null)
        );
      })
    );
  }

  getTabByName(name: string): Observable<DynamicTabDto | null> {
    return this.apiService.get<DynamicTabDto>(`/api/DynamicTab/by-name/${name}`).pipe(
      map(response => response),
      catchError(error => {
        console.error(`Error fetching tab by name ${name}:`, error);
        return this.getAllActiveTabs().pipe(
          map(tabs => tabs.find(tab => tab.name === name) || null)
        );
      })
    );
  }

  getTabsByDataSource(dataSource: string): Observable<DynamicTabDto[]> {
    return this.apiService.get<DynamicTabDto[]>(`/api/DynamicTab/by-datasource/${dataSource}`).pipe(
      map(response => response),
      catchError(error => {
        console.error(`Error fetching tabs by data source ${dataSource}:`, error);
        return this.getAllActiveTabs().pipe(
          map(tabs => tabs.filter(tab => tab.dataSource === dataSource))
        );
      })
    );
  }

  createTab(tab: Omit<DynamicTabDto, 'id'>): Observable<DynamicTabDto> {
    const createDto = {
      name: tab.name,
      displayName: tab.displayName,
      description: tab.description,
      dataSource: tab.dataSource,
      icon: tab.icon,
      color: tab.color,
      sortOrder: tab.sortOrder,
      isActive: tab.isActive,
      isAdminOnly: tab.isAdminOnly,
      requiredPermission: tab.requiredPermission,
      databaseQuery: tab.databaseQuery
    };

    return this.apiService.post<DynamicTabDto>('/api/DynamicTab', createDto).pipe(
      map(response => response),
      catchError(error => {
        console.error('Error creating tab:', error);
        const newTab: DynamicTabDto = {
          ...tab,
          id: Date.now().toString()
        };
        return of(newTab);
      })
    );
  }

  updateTab(id: string, updates: Partial<DynamicTabDto>): Observable<DynamicTabDto | null> {
    const updateDto = {
      name: updates.name,
      displayName: updates.displayName,
      description: updates.description,
      dataSource: updates.dataSource,
      icon: updates.icon,
      color: updates.color,
      sortOrder: updates.sortOrder,
      isActive: updates.isActive,
      isAdminOnly: updates.isAdminOnly,
      requiredPermission: updates.requiredPermission,
      databaseQuery: updates.databaseQuery
    };

    return this.apiService.put<DynamicTabDto>(`/api/DynamicTab/${id}`, updateDto).pipe(
      map(response => response),
      catchError(error => {
        console.error(`Error updating tab ${id}:`, error);
        return this.getTabById(id).pipe(
          map(tab => tab ? { ...tab, ...updates } : null)
        );
      })
    );
  }

  deleteTab(id: string): Observable<boolean> {
    return this.apiService.delete<any>(`/api/DynamicTab/${id}`).pipe(
      map(() => true),
      catchError(error => {
        console.error(`Error deleting tab ${id}:`, error);
        return of(false);
      })
    );
  }

  toggleTabStatus(id: string): Observable<boolean> {
    return this.apiService.post<any>(`/api/DynamicTab/${id}/toggle`, {}).pipe(
      map(() => true),
      catchError(error => {
        console.error(`Error toggling tab status ${id}:`, error);
        return of(false);
      })
    );
  }

  reorderTabs(reorderData: any[]): Observable<void> {
    return this.apiService.post<any>('/api/DynamicTab/reorder', reorderData).pipe(
      map(() => void 0),
      catchError(error => {
        console.error('Error reordering tabs:', error);
        return of(void 0);
      })
    );
  }

  createDynamicTab(tabData: any): Observable<{ success: boolean; data?: DynamicTabDto; message?: string }> {
    return this.apiService.post<DynamicTabDto>('/api/DynamicTab', tabData).pipe(
      map(response => ({ success: true, data: response })),
      catchError(error => {
        console.error('Error creating dynamic tab:', error);
        return of({ success: false, message: 'Failed to create dynamic tab' });
      })
    );
  }

  updateDynamicTab(id: string, tabData: any): Observable<{ success: boolean; data?: DynamicTabDto; message?: string }> {
    return this.apiService.put<DynamicTabDto>(`/api/DynamicTab/${id}`, tabData).pipe(
      map(response => ({ success: true, data: response })),
      catchError(error => {
        console.error('Error updating dynamic tab:', error);
        return of({ success: false, message: 'Failed to update dynamic tab' });
      })
    );
  }

  deleteDynamicTab(id: string): Observable<{ success: boolean; message?: string }> {
    return this.apiService.delete<any>(`/api/DynamicTab/${id}`).pipe(
      map(() => ({ success: true })),
      catchError(error => {
        console.error('Error deleting dynamic tab:', error);
        return of({ success: false, message: 'Failed to delete dynamic tab' });
      })
    );
  }
}
