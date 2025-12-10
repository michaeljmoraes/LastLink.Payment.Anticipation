import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
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

  // Ajuste se sua API estiver em outra porta
  private readonly baseUrl = 'http://localhost:5274/api/v1/anticipations';

  // 📌 LISTAR POR CRIADOR
    listByCreator(creatorId: string): Observable<AnticipationDto[]> {
    return this.http.get<any>(`${this.baseUrl}?creatorId=${creatorId}`).pipe(
        map(res => res.data as AnticipationDto[])
    );
    }


  // 📌 CRIAR SOLICITAÇÃO
  create(request: CreateAnticipationRequest): Observable<CreateAnticipationResponse> {
    return this.http.post<CreateAnticipationResponse>(this.baseUrl, request);
  }

  // 📌 APROVAR SOLICITAÇÃO
  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, {});
  }

  // 📌 REJEITAR SOLICITAÇÃO
  reject(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, {});
  }

  // 📌 SIMULAÇÃO (corrigida)
  simulate(request: SimulationRequest): Observable<SimulationResponse> {
    const params = `creatorId=${request.creatorId}&grossAmount=${request.grossAmount}`;

    return this.http.get<any>(`${this.baseUrl}/simulate?${params}`).pipe(
      map(res => {
        const d = res.data;

        return {
          grossAmount: d.grossAmount,
          netAmount: d.netAmount,
          feeAmount: d.grossAmount - d.netAmount // fee calculada no front
        } as SimulationResponse;
      })
    );
  }
}
