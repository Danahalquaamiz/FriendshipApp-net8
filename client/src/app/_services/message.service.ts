import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/messages';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelpers';

// The MessageService is responsible for interacting with the backend API to fetch the messages and manage the paginated results.

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null); //paginatedResult: This is a reactive signal (signal) that holds the paginated results of the messages. It stores the messages along with pagination information (e.g., total items, current page, etc.).

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

  sendMessgae(username: string, content: string){
    return this.http.post<Message>(this.baseUrl + 'messages' , {recipientUsername: username, content})
  }

  deleteMessage(id: number){
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
