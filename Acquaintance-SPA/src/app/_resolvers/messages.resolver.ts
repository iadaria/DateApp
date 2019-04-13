import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Message } from '@angular/compiler/src/i18n/i18n_ast';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MessagesResolver implements Resolve<Message[]> {
    pageNumber = 1;
    pageSize = 5;
    messageContainer = 'Unread';

    constructor(private userService: UserService,
                private authService: AuthService,
                private router: Router,
                private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {
        // This automaticallly subscribes to the method so we don't need to subscribe.
        // But we need catch error therefore use the pipe method
        return this.userService.getMessages(this.authService.decodedToken.nameid,
            this.pageNumber,
            this.pageSize,
            this.messageContainer)
            .pipe(
                catchError(error => {
                    this.alertify.error('Проблемы при получении сообщения.');
                    this.router.navigate(['/home']);
                    return of(null);
                })
        );
    }
}
