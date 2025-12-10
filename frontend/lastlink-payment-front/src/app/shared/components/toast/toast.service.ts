import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  text: string;
  type: 'success' | 'error';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toast = signal<ToastMessage | null>(null);

  show(text: string, type: 'success' | 'error' = 'success') {
    this.toast.set({ text, type });

    // Remove automatically after 3 seconds
    setTimeout(() => this.toast.set(null), 3000);
  }
}
