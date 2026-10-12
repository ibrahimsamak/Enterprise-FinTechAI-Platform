import { Injectable, computed, inject, signal } from '@angular/core';
import { HubConnection } from '@microsoft/signalr';
import { HubFactory } from 'shared-ui';
import { FraudAlert, FraudCaseView } from './fraud.models';

export type FeedState = 'disconnected' | 'connecting' | 'live' | 'reconnecting';
@Injectable({ providedIn: 'root' })
export class FraudFeedService {
    private readonly hubs = inject(HubFactory)
    private connection: HubConnection | null = null

    private readonly _cases = signal<FraudCaseView[]>([])
    readonly cases = this._cases.asReadonly();
    readonly openCount = computed(()=> this.cases().filter(c=>c.resolution === null).length);
    readonly state = signal<FeedState>('disconnected')
    readonly error = signal<string | null>(null);

    async start(): Promise<void>{
        if (this.connection) return;
        const conn = this.hubs.create('/hubs/fraud');
        conn.on('fraudFlagged', (alert: FraudAlert)=>
            this._cases.update(list=>
                list.some(c=>c.alert.paymentId == alert.paymentId) ? list : [{alert, receivedAt: new Date(), resolution: null }, ...list]
            )
        );
        conn.onreconnecting(()=> this.state.set('reconnecting'))
        conn.onreconnected(()=>this.state.set('live'));
        conn.onclose(err=>{
            this.connection = null;
            this.state.set('disconnected')
            if(err) this.error.set('Feed Closed: '+ err.message)
        });

        this.connection = conn
        this.state.set('connecting')
        try{
            await conn.start();
            this.state.set('live');
            this.error.set(null);
        }catch(e){
            this.connection = null;
            this.state.set('disconnected');
            this.error.set(`Could not connect to /hubs/fraud - API down, or the token lacks the FraudOfficer role. (${String(e)})`);
        }
    }
     
    markResolved(paymentId: string, resolution: FraudCaseView['resolution']): void {
        this._cases.update(list => list.map(c => (c.alert.paymentId === paymentId ? { ...c, resolution } : c)));
    }
    
    async stop(): Promise<void> {
        await this.connection?.stop();
        this.connection = null;
        this._cases.set([]);
        this.state.set('disconnected');
    }
}