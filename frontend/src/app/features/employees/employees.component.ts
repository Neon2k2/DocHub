import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';
import { SelectionModel } from '@angular/cdk/collections';

interface Employee {
  id: string;
  name: string;
  email: string;
  department: string;
  designation: string;
  joiningDate: Date;
  status: 'active' | 'inactive' | 'terminated';
  dataSource: 'Upload' | 'Database';
  lastUpdated: Date;
}

@Component({
  selector: 'app-employees',
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule]
})
export class EmployeesComponent implements OnInit {
  // Component state
  isLoading = false;
  employees: Employee[] = [];
  
  // Search and filter
  searchTerm = '';
  departmentFilter = 'all';
  statusFilter = 'all';
  dataSourceFilter = 'all';
  
  // Filter options
  departments = [
    { value: 'all', label: 'All Departments' },
    { value: 'IT', label: 'Information Technology' },
    { value: 'HR', label: 'Human Resources' },
    { value: 'Finance', label: 'Finance' },
    { value: 'Marketing', label: 'Marketing' },
    { value: 'Operations', label: 'Operations' }
  ];
  
  statuses = [
    { value: 'all', label: 'All Statuses' },
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
    { value: 'terminated', label: 'Terminated' }
  ];
  
  dataSources = [
    { value: 'all', label: 'All Sources' },
    { value: 'Upload', label: 'Upload Data' },
    { value: 'Database', label: 'Database' }
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading = true;
    
    // Mock data for now
    setTimeout(() => {
      this.employees = [
        {
          id: '1',
          name: 'John Doe',
          email: 'john.doe@company.com',
          department: 'IT',
          designation: 'Senior Developer',
          joiningDate: new Date('2022-03-15'),
          status: 'active',
          dataSource: 'Upload',
          lastUpdated: new Date(Date.now() - 86400000)
        },
        {
          id: '2',
          name: 'Jane Smith',
          email: 'jane.smith@company.com',
          department: 'HR',
          designation: 'HR Manager',
          joiningDate: new Date('2021-08-20'),
          status: 'active',
          dataSource: 'Database',
          lastUpdated: new Date(Date.now() - 172800000)
        },
        {
          id: '3',
          name: 'Mike Johnson',
          email: 'mike.johnson@company.com',
          department: 'Finance',
          designation: 'Financial Analyst',
          joiningDate: new Date('2023-01-10'),
          status: 'active',
          dataSource: 'Upload',
          lastUpdated: new Date(Date.now() - 259200000)
        },
        {
          id: '4',
          name: 'Sarah Wilson',
          email: 'sarah.wilson@company.com',
          department: 'Marketing',
          designation: 'Marketing Specialist',
          joiningDate: new Date('2022-11-05'),
          status: 'inactive',
          dataSource: 'Database',
          lastUpdated: new Date(Date.now() - 345600000)
        },
        {
          id: '5',
          name: 'David Brown',
          email: 'david.brown@company.com',
          department: 'Operations',
          designation: 'Operations Manager',
          joiningDate: new Date('2020-06-12'),
          status: 'terminated',
          dataSource: 'Database',
          lastUpdated: new Date(Date.now() - 432000000)
        }
      ];
      
      this.isLoading = false;
    }, 1000);
  }

  // Employee actions
  addEmployee(): void {
    console.log('Adding new employee');
    // TODO: Implement employee creation
  }

  editEmployee(employee: Employee): void {
    console.log('Editing employee:', employee.id);
    // TODO: Implement employee editing
  }

  deleteEmployee(employee: Employee): void {
    console.log('Deleting employee:', employee.id);
    // TODO: Implement employee deletion
  }

  viewEmployee(employee: Employee): void {
    console.log('Viewing employee:', employee.id);
    // TODO: Implement employee viewing
  }

  // Utility methods
  getStatusColor(status: string): string {
    const colorMap: { [key: string]: string } = {
      'active': 'primary',
      'inactive': 'warn',
      'terminated': 'warn'
    };
    return colorMap[status] || 'primary';
  }

  getStatusIcon(status: string): string {
    const iconMap: { [key: string]: string } = {
      'active': 'check_circle',
      'inactive': 'pause_circle',
      'terminated': 'cancel'
    };
    return iconMap[status] || 'help';
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

  getTimeAgo(date: Date): string {
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
    
    if (diffInDays === 0) return 'Today';
    if (diffInDays === 1) return 'Yesterday';
    if (diffInDays < 7) return `${diffInDays} days ago`;
    if (diffInDays < 30) return `${Math.floor(diffInDays / 7)} weeks ago`;
    if (diffInDays < 365) return `${Math.floor(diffInDays / 30)} months ago`;
    return `${Math.floor(diffInDays / 365)} years ago`;
  }

  // Computed properties for template
  get activeEmployeesCount(): number {
    return this.employees.filter(e => e.status === 'active').length;
  }

  get uploadBasedCount(): number {
    return this.employees.filter(e => e.dataSource === 'Upload').length;
  }

  get databaseBasedCount(): number {
    return this.employees.filter(e => e.dataSource === 'Database').length;
  }

  // Filtering
  get filteredEmployees(): Employee[] {
    return this.employees.filter(employee => {
      const matchesSearch = !this.searchTerm || 
        employee.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        employee.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        employee.designation.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesDepartment = this.departmentFilter === 'all' || 
        employee.department === this.departmentFilter;
      
      const matchesStatus = this.statusFilter === 'all' || 
        employee.status === this.statusFilter;
      
      const matchesDataSource = this.dataSourceFilter === 'all' || 
        employee.dataSource === this.dataSourceFilter;
      
      return matchesSearch && matchesDepartment && matchesStatus && matchesDataSource;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.departmentFilter = 'all';
    this.statusFilter = 'all';
    this.dataSourceFilter = 'all';
  }

  // Bulk operations
  selectedEmployees: Employee[] = [];
  displayedColumns: string[] = ['select', 'name', 'department', 'designation', 'joiningDate', 'status', 'dataSource', 'lastUpdated', 'actions'];
  selection = new SelectionModel<Employee>(true, []);

  onEmployeeSelection(employee: Employee, checked: boolean): void {
    if (checked) {
      this.selectedEmployees.push(employee);
    } else {
      this.selectedEmployees = this.selectedEmployees.filter(e => e.id !== employee.id);
    }
  }

  isEmployeeSelected(employee: Employee): boolean {
    return this.selectedEmployees.some(e => e.id === employee.id);
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selectedEmployees = [];
    } else {
      this.selectedEmployees = [...this.filteredEmployees];
    }
  }

  isAllSelected() {
    const numSelected = this.selectedEmployees.length;
    const numRows = this.filteredEmployees.length;
    return numSelected === numRows;
  }

  bulkDelete(): void {
    if (this.selectedEmployees.length === 0) return;
    
    const confirmed = confirm(`Are you sure you want to delete ${this.selectedEmployees.length} employees?`);
    if (confirmed) {
      console.log('Bulk deleting employees:', this.selectedEmployees.map(e => e.id));
      // TODO: Implement bulk deletion
      this.selectedEmployees = [];
    }
  }

  bulkExport(): void {
    if (this.selectedEmployees.length === 0) return;
    
    console.log('Bulk exporting employees:', this.selectedEmployees.map(e => e.id));
    // TODO: Implement bulk export
  }
}
