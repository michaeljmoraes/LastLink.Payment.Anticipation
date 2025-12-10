export enum AnticipationStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2
}

export interface AnticipationDto {
  id: string;
  creatorId: string;
  grossAmount: number;
  netAmount: number;
  feeRate: number;
  status: AnticipationStatus;
  createdAt: string;
  decisionAt?: string | null;
}

export interface CreateAnticipationRequest {
  creatorId: string;
  grossAmount: number;
}

export interface CreateAnticipationResponse {
  success: boolean;
  anticipationId?: string;
  errors?: string[];
}

export interface SimulationRequest {
  creatorId: string;
  grossAmount: number;
}

export interface SimulationResponse {
  grossAmount: number;
  feeAmount: number;
  netAmount: number;
}
