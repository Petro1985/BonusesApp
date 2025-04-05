import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationEndpoint {
  private demoNotifications = [
    {
      id: 1,
      header: '',
      body: '',
      isRead: true,
      isPinned: true,
      date: ''
    }
  ];


  getNotificationEndpoint<T>(notificationId: number) {
    const notification = this.demoNotifications.find(val => val.id === notificationId);
    return this.createDemoResponse(notification as T, notification ? null : 404);
  }

  getNotificationsEndpoint<T>(page: number, pageSize: number) {
    return this.createDemoResponse(this.demoNotifications as T, null, page, pageSize);
  }

  getUnreadNotificationsEndpoint<T>(userId?: string) {
    const unreadNotifications = this.demoNotifications.filter(val => !val.isRead);
    return this.createDemoResponse(unreadNotifications as T, null, userId);
  }

  getNewNotificationsEndpoint<T>(lastNotificationDate?: Date) {
    return this.createDemoResponse(this.demoNotifications as T, null, lastNotificationDate);
  }

  getPinUnpinNotificationEndpoint(notificationId: number, isPinned?: boolean) {
    const notification = this.demoNotifications.find(val => val.id === notificationId);

    if (notification) {
      if (isPinned == null) {
        isPinned = !notification.isPinned;
      }

      notification.isPinned = isPinned;
      notification.isRead = true;

      return this.createDemoResponse(null);
    } else {
      return this.createDemoResponse(null, 404);
    }
  }

  getReadUnreadNotificationEndpoint(notificationIds: number[], isRead: boolean) {
    for (const notificationId of notificationIds) {
      const notification = this.demoNotifications.find(val => val.id === notificationId);

      if (notification)
        notification.isRead = isRead;
    }

    return this.createDemoResponse(null);
  }

  getDeleteNotificationEndpoint<T>(notificationId: number) {
    const notification = this.demoNotifications.find(val => val.id === notificationId);

    if (notification)
      this.demoNotifications = this.demoNotifications.filter(val => val.id !== notificationId);

    return this.createDemoResponse(notification as T, notification ? null : 404);
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private createDemoResponse<T>(data: T, errorCode: number | null = null, ...args: unknown[]): Observable<T> {
    if (errorCode == null)
      return of(data);
    else
      return throwError(() => new HttpErrorResponse({ status: errorCode, error: 'Demo. An error occurred' }));
  }
}
