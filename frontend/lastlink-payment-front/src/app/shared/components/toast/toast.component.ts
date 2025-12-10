import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast" *ngIf="toast()"
         [class.success]="toast()?.type === 'success'"
         [class.error]="toast()?.type === 'error'">
      {{ toast()?.text }}
    </div>
  `,
  styleUrls: ['./toast.component.scss']
})
export class ToastComponent {
  private service = inject(ToastService);
  toast = computed(() => this.service.toast());
}
