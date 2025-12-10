import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AnticipationService } from '../../../shared/services/anticipation.service';
import { SimulationResponse } from '../../../shared/models/anticipation.model';

@Component({
  selector: 'app-anticipation-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './anticipation-create.component.html',
  styleUrls: ['./anticipation-create.component.scss']
})
export class AnticipationCreateComponent {

  private fb = inject(FormBuilder);
  private service = inject(AnticipationService);

  form: FormGroup = this.fb.group({
    grossAmount: [null, [Validators.required, Validators.min(100)]]
  });

  readonly creatorId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';

  isSimulating = false;
  isSubmitting = false;

  simulation?: SimulationResponse;
  errorMessage = '';
  successMessage = '';

  // 🔎 Validação de campos
  showError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  // 🔥 SIMULAR
  simulate(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSimulating = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.simulation = undefined;

    const gross = this.form.value.grossAmount;

    this.service
      .simulate({ creatorId: this.creatorId, grossAmount: gross })
      .subscribe({
        next: sim => {
          this.simulation = sim;
          this.isSimulating = false;
        },
        error: err => {
          this.errorMessage =
            err?.error?.error ??
            'Simulation failed. Check the amount and try again.';
          this.isSimulating = false;
        }
      });
  }

  // 🚀 CRIAR SOLICITAÇÃO
  create(): void {

    if (!this.simulation) {
      this.errorMessage = 'Please run a simulation first.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const gross = this.form.value.grossAmount;

    this.service
      .create({ creatorId: this.creatorId, grossAmount: gross })
      .subscribe({
        next: resp => {
          if (resp.success) {
            this.successMessage =
              'Your advance request was created successfully and is now under review.';
            this.form.reset();
            this.simulation = undefined;
          } else {
            this.errorMessage =
              resp.errors?.join(', ') ??
              'Error creating request.';
          }

          this.isSubmitting = false;
        },

        error: err => {
          this.errorMessage =
            err?.error?.error ??
            'Unable to create the request.';
          this.isSubmitting = false;
        }
      });
  }
}
