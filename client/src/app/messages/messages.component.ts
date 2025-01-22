import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from '../_services/message.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from '../_models/messages';
import { RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [ButtonsModule, FormsModule, TimeagoModule, RouterLink, PaginationModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  messageService = inject(MessageService);
  container = 'Inbox'; // container: This variable controls which type of messages are displayed (e.g., Inbox, Outbox, Unread). Initially, it's set to 'Outbox'.
  pageNumber = 1; // The current page number for pagination.
  pageSize = 5; // The number of messages per page.
  message: any; // A placeholder for the message data, but it’s not directly used in the component code (this could be removed or used if needed).
  isOutbox = this.container === 'Outbox';

  ngOnInit(): void {
    this.loadMessages(); // This lifecycle hook calls the loadMessages() method when the component is initialized to fetch the messages based on the current container value (Outbox in this case).
  }

  // This method calls the messageService.getMessages() function to fetch the messages for the selected container (Inbox, Outbox, or Unread). 
  // It uses the pageNumber and pageSize to fetch the correct set of messages.
  loadMessages() {
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container);
  }


  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        this.messageService.paginatedResult.update(prev => {
          if (prev && prev.items) {
            prev.items.splice(prev.items.findIndex(m => m.id === id), 1);
            return prev;
          }
          return prev;
        })
      }
    })
  }

    // This method determines the route to navigate to for the message. If the container is 'Outbox',
    // // it returns a route for the recipient of the message; otherwise, it returns a route for the sender.
    getRoute(message: Message){
      if (this.container === 'Outbox') return `/members/${message.recipientUsername}`;
      else return `/members/${message.senderUsername}`;
    }
    // This method listens for changes in the page number due to pagination.
    // If the page number changes, it reloads the messages for the new page.
    pageChanged(event: any){
      if (this.pageNumber !== event.page) {
        this.pageNumber = event.page;
        this.loadMessages();
      }
    }
  }
