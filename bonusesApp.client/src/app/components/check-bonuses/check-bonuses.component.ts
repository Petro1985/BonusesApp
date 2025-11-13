import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { NgxMaskDirective } from 'ngx-mask';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { BonusesEndpoint } from '../../services/bonuses-endpoint.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { AuthService } from '../../services/auth.service';
import { Bonuses } from '../../models/bonuses.model';

@Component({
  selector: 'app-check-bonuses',
  templateUrl: './check-bonuses.component.html',
  styleUrl: './check-bonuses.component.scss',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule, NgxMaskDirective]
})
export class CheckBonusesComponent implements OnInit {
  private alertService = inject(AlertService);
  private bonusesService = inject(BonusesEndpoint);
  private translationService = inject(AppTranslationService);
  private authService = inject(AuthService);
  private router = inject(Router);

  phoneNumber: string = '';
  isLoading = false;
  bonuses: Bonuses | null = null;
  notFound = false;

  gT = (key: string) => this.translationService.getTranslation(key);

  // Проверка валидности номера телефона (должен содержать цифры, а не только префикс)
  isPhoneNumberValid(): boolean {
    if (!this.phoneNumber || this.phoneNumber.trim() === '') {
      return false;
    }
    // Проверяем, что есть цифры (убираем все нецифровые символы и проверяем длину)
    const digitsOnly = this.phoneNumber.replace(/\D/g, '');
    return digitsOnly.length >= 10;
  }

  ngOnInit() {
    // Если пользователь авторизован, редиректим на страницу home
    if (this.authService.isLoggedIn) {
      this.router.navigate(['/home']);
    }
  }

  checkBonuses() {
    if (!this.isPhoneNumberValid()) {
      this.alertService.showMessage(
        this.gT('checkBonuses.PhoneNumberRequired'),
        this.gT('checkBonuses.EnterPhoneNumber'),
        MessageSeverity.warn
      );
      return;
    }

    // Очистка номера телефона от лишних символов (оставляем только цифры)
    let cleanPhone = this.phoneNumber.replace(/\D/g, '');
    
    // Если номер не начинается с 7, добавляем 7
    if (!cleanPhone.startsWith('7')) {
      cleanPhone = '7' + cleanPhone;
    }
    
    // Добавляем префикс +
    cleanPhone = '+' + cleanPhone;

    this.isLoading = true;
    this.bonuses = null;
    this.notFound = false;

    // Используем публичный endpoint для проверки бонусов
    this.bonusesService.checkBonuses(cleanPhone).subscribe({
      next: (bonuses) => {
        this.isLoading = false;
        this.bonuses = bonuses;
        this.notFound = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.bonuses = null;
        
        // Проверяем, является ли ошибка 404 (клиент не найден)
        if (error.status === 404) {
          this.notFound = true;
          this.alertService.showMessage(
            this.gT('checkBonuses.ClientNotFound'),
            this.gT('checkBonuses.ClientNotFoundMessage'),
            MessageSeverity.info
          );
        } else {
          this.notFound = false;
          this.alertService.showStickyMessage(
            this.gT('checkBonuses.Error'),
            this.gT('checkBonuses.ErrorMessage'),
            MessageSeverity.error,
            error
          );
        }
      }
    });
  }

  reset() {
    this.phoneNumber = '';
    this.bonuses = null;
    this.notFound = false;
  }

  // Форматирование даты в формате "dd.MM.yyyy HH:mm"
  formatLastUpdate(date: Date | string | null | undefined): string {
    if (!date) {
      return '';
    }

    const d = new Date(date);
    if (isNaN(d.getTime())) {
      return '';
    }

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');

    return `${day}.${month}.${year} ${hours}:${minutes}`;
  }
}

