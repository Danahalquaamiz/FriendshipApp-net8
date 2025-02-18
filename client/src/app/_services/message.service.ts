import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/messages';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelpers';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';
import { BusyService } from './busy.service';

// The MessageService is responsible for interacting with the backend API to fetch the messages and manage the paginated results.

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  private busyService = inject(BusyService);
  hubUrl = environment.hubsUrl;
  hubConnection?: HubConnection;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null); //paginatedResult: This is a reactive signal (signal) that holds the paginated results of the messages. It stores the messages along with pagination information (e.g., total items, current page, etc.).
  messageThread = signal<Message[]>([]);

  createHubConnection(user: User, otherUsername: string) {
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .catch(error => console.log(error))
      .finally(() => this.busyService.idle);

    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages)
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message])
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          })
          return messages;
        })
      }
    })
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  // This method is used to make an HTTP GET request to the API endpoint 'messages' to fetch the messages.
  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize); // set the pagination headers (like pageNumber and pageSize) for the API request.
    params = params.append('Container', container); // Container Filter: It appends the Container parameter (which could be 'Inbox', 'Outbox', or 'Unread') to the request to filter the messages accordingly.

    return this.http.get<Message[]>(this.baseUrl + 'messages', { observe: 'response', params })
      .subscribe({
        next: response => setPaginatedResponse(response, this.paginatedResult) // process the response and store the paginated results in the paginatedResult signal. It handles setting the current page, total items, and the actual message data.
      })
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', { recipientUsername: username, content })
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
