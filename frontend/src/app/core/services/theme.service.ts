import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkThemeSubject = new BehaviorSubject<boolean>(false);
  public isDarkTheme$: Observable<boolean> = this.darkThemeSubject.asObservable();

  constructor() {
    this.loadThemePreference();
  }

  private loadThemePreference(): void {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      this.darkThemeSubject.next(savedTheme === 'dark');
    } else {
      // Check system preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.darkThemeSubject.next(prefersDark);
    }
  }

  toggleTheme(): void {
    const currentTheme = this.darkThemeSubject.value;
    const newTheme = !currentTheme;
    
    this.darkThemeSubject.next(newTheme);
    localStorage.setItem('theme', newTheme ? 'dark' : 'light');
    
    // Apply theme to body
    if (newTheme) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }

  setTheme(isDark: boolean): void {
    this.darkThemeSubject.next(isDark);
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    
    if (isDark) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }

  getCurrentTheme(): boolean {
    return this.darkThemeSubject.value;
  }
}
