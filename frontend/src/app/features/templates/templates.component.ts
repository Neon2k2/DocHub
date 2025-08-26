import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';

interface LetterTemplate {
  id: string;
  name: string;
  letterType: string;
  description: string;
  dataSource: 'Upload' | 'Database';
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

@Component({
  selector: 'app-templates',
  templateUrl: './templates.component.html',
  styleUrls: ['./templates.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule]
})
export class TemplatesComponent implements OnInit {
  // Component state
  isLoading = false;
  templates: LetterTemplate[] = [];
  
  // Search and filter
  searchTerm = '';
  typeFilter = 'all';
  
  // Template types
  templateTypes = [
    { value: 'all', label: 'All Types' },
    { value: 'transfer', label: 'Transfer Letters' },
    { value: 'experience', label: 'Experience Letters' },
    { value: 'confirmation', label: 'Confirmation Letters' },
    { value: 'termination', label: 'Termination Letters' }
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.isLoading = true;
    
    // Mock data for now
    setTimeout(() => {
      this.templates = [
        {
          id: '1',
          name: 'Transfer Letter Template',
          letterType: 'transfer',
          description: 'Standard template for employee transfer notifications',
          dataSource: 'Upload',
          isActive: true,
          createdAt: new Date(Date.now() - 86400000),
          updatedAt: new Date(Date.now() - 86400000)
        },
        {
          id: '2',
          name: 'Experience Letter Template',
          letterType: 'experience',
          description: 'Template for employee experience certificates',
          dataSource: 'Upload',
          isActive: true,
          createdAt: new Date(Date.now() - 172800000),
          updatedAt: new Date(Date.now() - 172800000)
        },
        {
          id: '3',
          name: 'Confirmation Letter Template',
          letterType: 'confirmation',
          description: 'Template for employment confirmation letters',
          dataSource: 'Database',
          isActive: true,
          createdAt: new Date(Date.now() - 259200000),
          updatedAt: new Date(Date.now() - 259200000)
        }
      ];
      
      this.isLoading = false;
    }, 1000);
  }

  // Template actions
  createTemplate(): void {
    console.log('Creating new template');
    // TODO: Implement template creation
  }

  editTemplate(template: LetterTemplate): void {
    console.log('Editing template:', template.id);
    // TODO: Implement template editing
  }

  deleteTemplate(template: LetterTemplate): void {
    console.log('Deleting template:', template.id);
    // TODO: Implement template deletion
  }

  toggleTemplateStatus(template: LetterTemplate): void {
    console.log('Toggling template status:', template.id);
    // TODO: Implement status toggle
  }

  // Utility methods
  getTemplateIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'transfer': '🔄',
      'experience': '📋',
      'confirmation': '✅',
      'termination': '🚪',
      'default': '📄'
    };
    return iconMap[type] || iconMap['default'];
  }

  getDataSourceColor(dataSource: string): string {
    return dataSource === 'Upload' ? 'primary' : 'accent';
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  // Computed properties for template
  get activeTemplatesCount(): number {
    return this.templates.filter(t => t.isActive).length;
  }

  get uploadBasedCount(): number {
    return this.templates.filter(t => t.dataSource === 'Upload').length;
  }

  get databaseBasedCount(): number {
    return this.templates.filter(t => t.dataSource === 'Database').length;
  }

  // Filtering
  get filteredTemplates(): LetterTemplate[] {
    return this.templates.filter(template => {
      const matchesSearch = !this.searchTerm || 
        template.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        template.description.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesType = this.typeFilter === 'all' || 
        template.letterType === this.typeFilter;
      
      return matchesSearch && matchesType;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.typeFilter = 'all';
  }
}
