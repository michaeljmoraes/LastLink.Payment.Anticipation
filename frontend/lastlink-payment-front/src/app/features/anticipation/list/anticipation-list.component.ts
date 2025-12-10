import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; // Required for routerLink
import { AnticipationService } from '../../../shared/services/anticipation.service';
import { AnticipationDto, AnticipationStatus } from '../../../shared/models/anticipation.model';

@Component({
  selector: 'app-anticipation-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule // <-- Without this, routerLink does NOT work
  ],
  templateUrl: './anticipation-list.component.html',
  styleUrls: ['./anticipation-list.component.scss']
})
export class AnticipationListComponent implements OnInit {

  /* ==================================================
     Dependencies
     ================================================== */
  private service = inject(AnticipationService);

  /* ==================================================
     Component State
     ================================================== */
  anticipations: AnticipationDto[] = [];
  isLoading = false;
  errorMessage = '';

  /** Creator used for V1 mock scenarios */
  readonly creatorId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';

  /** Expose enum for the template */
  AnticipationStatus = AnticipationStatus;

  /* ==================================================
     Lifecycle
     ================================================== */
  ngOnInit(): void {
    this.load();
  }

  /* ==================================================
     Load anticipations from API
     ================================================== */
  load(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.service.listByCreator(this.creatorId).subscribe({
      next: data => {
        this.anticipations = data ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage =
          'We could not load your anticipations. Try again in a moment.';
        this.isLoading = false;
      }
    });
  }

  /* ==================================================
     Aggregated metrics for UI dashboards
     ================================================== */
  get totalRequested(): number {
    return this.anticipations.reduce((acc, a) => acc + a.grossAmount, 0);
  }

  get totalApproved(): number {
    return this.anticipations
      .filter(a => a.status === AnticipationStatus.Approved)
      .reduce((acc, a) => acc + a.netAmount, 0);
  }

  get totalPending(): number {
    return this.anticipations
      .filter(a => a.status === AnticipationStatus.Pending)
      .reduce((acc, a) => acc + a.grossAmount, 0);
  }

  get totalRejected(): number {
    return this.anticipations
      .filter(a => a.status === AnticipationStatus.Rejected)
      .reduce((acc, a) => acc + a.grossAmount, 0);
  }
  /**
   * Total amount available for withdrawal.
   * In future versions this will come from the Wallet service.
   */
  get totalAvailable(): number {
    return this.totalApproved;
  }

  /* ==================================================
     UI Helpers (labels + CSS classes)
     ================================================== */
  getStatusLabel(status: AnticipationStatus): string {
    switch (status) {
      case AnticipationStatus.Pending:
        return 'Pending review';
      case AnticipationStatus.Approved:
        return 'Approved';
      case AnticipationStatus.Rejected:
        return 'Rejected';
      default:
        return 'Unknown';
    }
  }

  getStatusClass(status: AnticipationStatus): string {
    switch (status) {
      case AnticipationStatus.Pending:
        return 'badge badge-pending';
      case AnticipationStatus.Approved:
        return 'badge badge-approved';
      case AnticipationStatus.Rejected:
        return 'badge badge-rejected';
      default:
        return 'badge';
    }
  }

  /* ==================================================
     Workflow Actions (Approve / Reject)
     ================================================== */
  approve(id: string): void {
    this.service.approve(id).subscribe({
      next: () => this.load(),
      error: err => {
        console.error('Approval error:', err);
        alert(err?.error?.error ?? 'Unable to approve the request.');
      }
    });
  }

  reject(id: string): void {
    this.service.reject(id).subscribe({
      next: () => this.load(),
      error: err => {
        console.error('Rejection error:', err);
        alert(err?.error?.error ?? 'Unable to reject the request.');
      }
    });
  }
}
