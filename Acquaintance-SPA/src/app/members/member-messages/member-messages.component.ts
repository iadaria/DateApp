import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { AuthService } from 'src/app/_services/auth.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService,
              private authService: AuthService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }
  // tap - allows us to do something before we subscribe to this particular method
  loadMessages() {
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(this.authService.decodedToken.nameid,
      this.recipientId)
        .pipe(
          tap(messages => {
            // for (let i = 0; i < messages.length; i++) {
              for (const message of messages) {
                if (message.isRead === false && message.recipientId === currentUserId) {
                  this.userService.markAsRead(currentUserId, message.id);
                }
            }
          })
        )
        .subscribe(messages => {
          this.messages = messages;
        }, error => {
          this.alertify.error(error);
        });
  }

  sendMessage() {
    // this.newMessage.senderId = this.authService.decodedToken.nameid;
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid,
      this.newMessage)
      .subscribe((message: Message) => {
        // debugger;
        this.messages.unshift(message);
        if (this.newMessage != null) {
          this.newMessage.content = '';
        }
      }, error => {
        this.alertify.error(error);
      });
  }
}
