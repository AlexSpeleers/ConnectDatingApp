import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../../types/user';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastService } from './toast-service';
import { Message } from '../../types/message';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubLUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  public onlineUsers = signal<string[]>([]);
  private toastService = inject(ToastService);

  public get IsHubConnected() {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }

  CreateHubConntection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubLUrl + 'presence', {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();
    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('UserIsOnline', (userId) => {
      this.onlineUsers.update((users) => [...users, userId]);
    });

    this.hubConnection.on('UserIsOffline', (userId) => {
      this.onlineUsers.update((users) => users.filter((x) => x !== userId));
    });

    this.hubConnection.on('GetOnlineUsers', (userIds) => {
      this.onlineUsers.set(userIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      this.toastService.Info(
        message.senderDisplayName + ' has sent you a message',
        10000,
        message.senderImageUrl,
        `/members/${message.senderId}/messages`,
      );
    });
  }

  StopConnection() {
    if ((this, this.hubConnection?.state === HubConnectionState.Connected)) {
      this.hubConnection.stop().catch((error) => console.log(error));
    }
  }
}
