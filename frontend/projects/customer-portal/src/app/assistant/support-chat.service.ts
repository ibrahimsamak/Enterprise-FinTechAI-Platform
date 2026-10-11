import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { HubFactory } from 'shared-ui';
export interface SupportCitation { docId: string; section: string; title: string; }
export interface SupportStreamItem { delta: string | null; citations: SupportCitation[] | null; }

@Injectable({ providedIn: 'root' })
export class SupportChatService {
    private readonly hubs = inject(HubFactory)
    private connection: HubConnection | null = null;
    async ask(question: string, onItem: (item: SupportStreamItem)=> void): Promise<void>{
        const conn = await this.connected();
        return new Promise<void>((resolve, reject)=>{
            conn.stream<SupportStreamItem>('Ask', question).subscribe({
                next: onItem, complete: resolve, error:reject
            });
        })
    }

    async stop(): Promise<void> {
        await this.connection?.stop();
        this.connection = null;
    }
    
    private async connected(): Promise<HubConnection> {
        this.connection??= this.hubs.create('/hubs/support');
        if(this.connection.state === HubConnectionState.Disconnected) await this.connection.start();
        return this.connection;
    }
}