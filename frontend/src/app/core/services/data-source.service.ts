import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { NotificationService } from './notification.service';

export interface DataSourceConfiguration {
  id: string;
  name: string;
  type: 'template' | 'employee';
  dataSource: 'upload' | 'database';
  isActive: boolean;
  lastUpdated: Date;
  description?: string;
}

export interface DataSourceStats {
  totalSources: number;
  uploadSources: number;
  databaseSources: number;
  activeSources: number;
}

@Injectable({
  providedIn: 'root'
})
export class DataSourceService {
  private _configurations = new BehaviorSubject<DataSourceConfiguration[]>([]);
  private _stats = new BehaviorSubject<DataSourceStats>({
    totalSources: 0,
    uploadSources: 0,
    databaseSources: 0,
    activeSources: 0
  });
  private _isLoading = new BehaviorSubject<boolean>(false);

  public configurations$ = this._configurations.asObservable();
  public stats$ = this._stats.asObservable();
  public isLoading$ = this._isLoading.asObservable();

  constructor(
    private apiService: ApiService,
    private notificationService: NotificationService
  ) {
    this.loadConfigurations();
  }

  /**
   * Load all data source configurations
   */
  loadConfigurations(): Observable<DataSourceConfiguration[]> {
    this._isLoading.next(true);
    
    return this.apiService.getDataSources().pipe(
      map(response => {
        const templates = response.data || [];
        const configurations: DataSourceConfiguration[] = templates.map((template: any) => ({
          id: template.id,
          name: template.name || template.displayName,
          type: 'template',
          dataSource: template.dataSource?.toLowerCase() === 'upload' ? 'upload' : 'database',
          isActive: template.isActive,
          lastUpdated: new Date(template.updatedAt || template.createdAt),
          description: template.description
        }));

        this._configurations.next(configurations);
        this.updateStats(configurations);
        this._isLoading.next(false);
        
        return configurations;
      })
    );
  }

  /**
   * Toggle data source for a specific configuration
   */
  toggleDataSource(id: string, newSource: 'upload' | 'database'): Observable<boolean> {
    this._isLoading.next(true);
    
    return this.apiService.toggleDataSource(id, newSource).pipe(
      map(response => {
        if (response.success) {
          // Update local state
          const currentConfigs = this._configurations.value;
          const updatedConfigs = currentConfigs.map(config => 
            config.id === id 
              ? { ...config, dataSource: newSource, lastUpdated: new Date() }
              : config
          );
          
          this._configurations.next(updatedConfigs);
          this.updateStats(updatedConfigs);
          this._isLoading.next(false);
          
          this.notificationService.showSuccess(
            'Data Source Updated',
            `Successfully switched to ${newSource} data source`
          );
          
          return true;
        } else {
          this._isLoading.next(false);
          this.notificationService.showError(
            'Update Failed',
            'Failed to update data source configuration'
          );
          return false;
        }
      })
    );
  }

  /**
   * Get configurations by data source type
   */
  getConfigurationsBySource(source: 'upload' | 'database'): Observable<DataSourceConfiguration[]> {
    return this.configurations$.pipe(
      map(configs => configs.filter(config => config.dataSource === source))
    );
  }

  /**
   * Get active configurations
   */
  getActiveConfigurations(): Observable<DataSourceConfiguration[]> {
    return this.configurations$.pipe(
      map(configs => configs.filter(config => config.isActive))
    );
  }

  /**
   * Bulk toggle multiple configurations
   */
  bulkToggleDataSource(ids: string[], newSource: 'upload' | 'database'): Observable<boolean> {
    this._isLoading.next(true);
    
    const togglePromises = ids.map(id => 
      this.apiService.toggleDataSource(id, newSource).toPromise()
    );
    
    return new Observable(observer => {
      Promise.all(togglePromises)
        .then(responses => {
          const allSuccessful = responses.every(response => response?.success);
          
          if (allSuccessful) {
            // Update local state
            const currentConfigs = this._configurations.value;
            const updatedConfigs = currentConfigs.map(config => 
              ids.includes(config.id)
                ? { ...config, dataSource: newSource, lastUpdated: new Date() }
                : config
            );
            
            this._configurations.next(updatedConfigs);
            this.updateStats(updatedConfigs);
            
            this.notificationService.showSuccess(
              'Bulk Update Successful',
              `Updated ${ids.length} configurations to ${newSource} data source`
            );
            
            observer.next(true);
          } else {
            this.notificationService.showError(
              'Bulk Update Failed',
              'Some configurations failed to update'
            );
            observer.next(false);
          }
          
          this._isLoading.next(false);
          observer.complete();
        })
        .catch(error => {
          console.error('Error in bulk toggle:', error);
          this.notificationService.showError(
            'Bulk Update Error',
            'An error occurred during bulk update'
          );
          this._isLoading.next(false);
          observer.next(false);
          observer.complete();
        });
    });
  }

  /**
   * Get data source statistics
   */
  getStats(): DataSourceStats {
    return this._stats.value;
  }

  /**
   * Refresh configurations from server
   */
  refresh(): void {
    this.loadConfigurations().subscribe();
  }

  /**
   * Update internal statistics
   */
  private updateStats(configurations: DataSourceConfiguration[]): void {
    const stats: DataSourceStats = {
      totalSources: configurations.length,
      uploadSources: configurations.filter(c => c.dataSource === 'upload').length,
      databaseSources: configurations.filter(c => c.dataSource === 'database').length,
      activeSources: configurations.filter(c => c.isActive).length
    };
    
    this._stats.next(stats);
  }

  /**
   * Get recommended data source based on template type
   */
  getRecommendedDataSource(templateType: string): 'upload' | 'database' {
    // Basic logic - can be enhanced based on business rules
    const dynamicTypes = ['bulk', 'custom', 'import'];
    const staticTypes = ['standard', 'template', 'form'];
    
    if (dynamicTypes.some(type => templateType.toLowerCase().includes(type))) {
      return 'upload';
    } else if (staticTypes.some(type => templateType.toLowerCase().includes(type))) {
      return 'database';
    }
    
    return 'upload'; // Default to upload
  }

  /**
   * Validate data source compatibility
   */
  validateDataSourceCompatibility(templateId: string, targetSource: 'upload' | 'database'): Observable<{valid: boolean, issues: string[]}> {
    // Mock implementation - would typically check field mappings, data availability, etc.
    return new Observable(observer => {
      setTimeout(() => {
        const issues: string[] = [];
        
        // Simulate validation logic
        if (targetSource === 'database') {
          // Check if database has required fields
          const currentConfig = this._configurations.value.find(c => c.id === templateId);
          if (currentConfig?.name.toLowerCase().includes('custom')) {
            issues.push('Custom templates may not have all required database mappings');
          }
        }
        
        observer.next({
          valid: issues.length === 0,
          issues
        });
        observer.complete();
      }, 500);
    });
  }
}
