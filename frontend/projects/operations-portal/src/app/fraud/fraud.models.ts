export interface FraudAlert {
  id: string;
  occurredOn: string;
  correlationId: string;
  paymentId: string;
  customerId: string;
  amount: number;
  currency: string;
  country: string;
  score: number;
  reasons: string[];
  investigation: string;
}

export type Resolution = 'Approve' | 'Escalate';

export interface FraudCaseView {
  alert: FraudAlert;
  receivedAt: Date;
  resolution: Resolution | 'ResolvedElsewhere' | null;
}
