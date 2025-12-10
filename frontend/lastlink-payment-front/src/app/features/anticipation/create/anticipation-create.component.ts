import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AnticipationService } from '../../../shared/services/anticipation.service';
import { SimulationResponse } from '../../../shared/models/anticipation.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-anticipation-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './anticipation-create.component.html',
  styleUrls: ['./anticipation-create.component.scss']
})
export class AnticipationCreateComponent {

  /* ============================
     Dependencies
     ============================ */
  private fb = inject(FormBuilder);
  private service = inject(AnticipationService);
  private toast = inject(ToastService);
  private router = inject(Router);

  /* ============================
     Form definition
     ============================ */
  form: FormGroup = this.fb.group({
    grossAmount: [null, [Validators.required, Validators.min(100)]]
  });

  readonly creatorId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';

  /* ============================
     Component state
     ============================ */
  isSimulating = false;
  isSubmitting = false;

  simulation?: SimulationResponse;

  /* ============================
     Field validation helper
     ============================ */
  showError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  /* ============================
     Simulation action
     ============================ */
  simulate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.show('Please enter a valid amount.', 'error');
      return;
    }

    this.isSimulating = true;
    this.simulation = undefined;

    const gross = this.form.value.grossAmount;

    this.service.simulate({ creatorId: this.creatorId, grossAmount: gross })
      .subscribe({
        next: sim => {
          this.simulation = sim;
          this.isSimulating = false;
          this.toast.show('Simulation updated.', 'success');
        },
        error: err => {
          this.toast.show(
            err?.error?.error ?? 'Simulation failed. Check the amount.',
            'error'
          );
          this.isSimulating = false;
        }
      });
  }

  /* ============================
     Create anticipation action
     ============================ */
  create(): void {
    if (!this.simulation) {
      this.toast.show('Please run a simulation first.', 'error');
      return;
    }

    this.isSubmitting = true;

    const gross = this.form.value.grossAmount;

    this.service.create({ creatorId: this.creatorId, grossAmount: gross })
      .subscribe({
        next: resp => {
          if (resp.success) {
            this.toast.show(
              'Your advance request was created successfully.',
              'success'
            );

            // Reset form and simulation
            this.form.reset();
            this.simulation = undefined;

            // Navigate back to list in 700ms (smooth UX)
            setTimeout(() => {
              this.router.navigate(['/anticipations']);
            }, 700);

          } else {
            this.toast.show(
              resp.errors?.join(', ') ?? 'Error creating request.',
              'error'
            );
          }

          this.isSubmitting = false;
        },

        error: err => {
          this.toast.show(
            err?.error?.error ?? 'Unable to create the request.',
            'error'
          );
          this.isSubmitting = false;
        }
      });
  }
}
