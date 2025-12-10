import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

import {
  AnticipationDto,
  CreateAnticipationRequest,
  CreateAnticipationResponse,
  SimulationRequest,
  SimulationResponse
} from '../models/anticipation.model';

@Injectable({
  providedIn: 'root'
})
export class AnticipationService {
  private http = inject(HttpClient);

  // Base API URL provided by the selected Angular environment (local or docker)
  private readonly baseUrl = environment.apiUrl;

  // Retrieve all anticipations for a given creator
  listByCreator(creatorId: string): Observable<AnticipationDto[]> {
    return this.http.get<any>(`${this.baseUrl}?creatorId=${creatorId}`).pipe(
      map(res => res.data as AnticipationDto[])
    );
  }

  // Create a new anticipation request
  create(request: CreateAnticipationRequest): Observable<CreateAnticipationResponse> {
    return this.http.post<CreateAnticipationResponse>(this.baseUrl, request);
  }

  // Approve an anticipation request
  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, {});
  }

  // Reject an anticipation request
  reject(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, {});
  }

  // Perform anticipation simulation based on gross amount and creator
  simulate(request: SimulationRequest): Observable<SimulationResponse> {
    const params = `creatorId=${request.creatorId}&grossAmount=${request.grossAmount}`;

    return this.http.get<any>(`${this.baseUrl}/simulate?${params}`).pipe(
      map(res => {
        const d = res.data;

        return {
          grossAmount: d.grossAmount,
          netAmount: d.netAmount,
          feeAmount: d.grossAmount - d.netAmount
        } as SimulationResponse;
      })
    );
  }
}
