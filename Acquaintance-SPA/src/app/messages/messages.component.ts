import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { Pagination, PaginatedResult } from '../_models/Pagination';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  user: User = JSON.parse(localStorage.getItem('user'));
  messageContainer = 'Unread';
  pagination: Pagination;

  constructor(private userService: UserService,
              private authService: AuthService,
              private alertify: AlertifyService,
              private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data.messages.result;
      this.pagination = data.messages.pagination;
    });
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    // console.log(event.page);
    this.loadMessages();
  }

  loadMessages() {
    // Use to subscribe because it's returning an observable
    this.userService
      .getMessages(this.authService.decodedToken.nameid,
        this.pagination.currentPage,
        this.pagination.itemsPerPage,
        this.messageContainer)
        .subscribe((res: PaginatedResult<Message[]>) => {
          this.messages = res.result;
          this.pagination = res.pagination;
        }, error => {
          this.alertify.error(error);
        });
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Are you sure you want to delete this message', () => {
      this.userService.deleteMessage(id, this.authService.decodedToken.nameid)
        .subscribe(() => {
          this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
          this.alertify.success('Сообщение было удалено'); // ('Message has been deleted');
        }, error => {
          this.alertify.error('Ошибка удаления сообщения'); // ('Failed to delete the message');
        });
    });
  }
}
