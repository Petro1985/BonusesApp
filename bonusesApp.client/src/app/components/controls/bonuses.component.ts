import { Component, OnInit, OnDestroy, TemplateRef, inject, input, viewChild } from '@angular/core';
import {CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbModal, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { TableColumn, NgxDatatableModule } from '@siemens/ngx-datatable';

import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { LocalStoreManager } from '../../services/local-store-manager.service';
import { Utilities } from '../../services/utilities';
import { SearchBoxComponent } from './search-box.component';
import { AutofocusDirective } from '../../directives/autofocus.directive';
import {Bonuses} from '../../models/bonuses.model';
import {BonusesEndpoint} from '../../services/bonuses-endpoint.service';
import { NgxMaskDirective } from 'ngx-mask';

interface Todo {
  $$index?: number;
  completed: boolean;
  important: boolean;
  name: string;
  setting: string;
  description: string;
}

@Component({
  selector: 'app-bonuses',
  templateUrl: './bonuses.component.html',
  styleUrl: './bonuses.component.scss',
  standalone: true,
  imports: [
    SearchBoxComponent, NgxDatatableModule, FormsModule,
    AutofocusDirective, NgbTooltip, NgClass,
    TranslateModule, CommonModule, NgxMaskDirective ]
})
export class BonusesComponent implements OnInit, OnDestroy {
  private alertService = inject(AlertService);
  private translationService = inject(AppTranslationService);
  private localStorage = inject(LocalStoreManager);
  private authService = inject(AuthService);
  private modalService = inject(NgbModal);
  private bonusesService = inject(BonusesEndpoint);

// todo: вероятно не нужно
  public static readonly DBKeyBonuses = 'bonuses.todo_list';

  columns: TableColumn[] = [];
  rows: Bonuses[] = [];
  rowsCache: Bonuses[] = [];
  editing: Record<string, boolean> = {};
  bonusEdit: Partial<Bonuses> = {};
  isDataLoaded = false;
  loadingIndicator = true;
  formResetToggle = true;
  private _currentUserId: string | undefined;
  private searchString: string = '';

  offset: number = 0;
  limit: number = 10;

  get currentUserId() {
    if (this.authService.currentUser) {
      this._currentUserId = this.authService.currentUser.id;
    }

    return this._currentUserId;
  }

  readonly verticalScrollbar = input(false);

  // readonly statusHeaderTemplate = viewChild.required<TemplateRef<unknown>>('statusHeaderTemplate');
  //
  // readonly statusTemplate = viewChild.required<TemplateRef<unknown>>('statusTemplate');

  readonly nameTemplate = viewChild.required<TemplateRef<unknown>>('nameTemplate');

  readonly settingTemplate = viewChild.required<TemplateRef<unknown>>('settingTemplate');

  readonly readOnlyTemplate = viewChild.required<TemplateRef<unknown>>('readOnlyTemplate');

  readonly actionsTemplate = viewChild.required<TemplateRef<unknown>>('actionsTemplate');

  readonly editorModalTemplate = viewChild.required<TemplateRef<unknown>>('editorModal');

  ngOnInit() {
    this.loadingIndicator = true;

    this.getBonuses(this.offset, this.limit, this.searchString)

    const gT = (key: string) => this.translationService.getTranslation(key);

    this.columns = [
      {
        prop: 'phoneNumber',
        name: gT('bonuses.management.PhoneNumber'),
        width: 200,
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false
      },
      {
        prop: 'name',
        name: gT('bonuses.management.Name'),
        width: 300,
        cellTemplate: this.nameTemplate(),
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false
      },
      {
        prop: 'totalCounter',
        name: gT('bonuses.management.TotalCounter'),
        width: 160,
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false,
        cellTemplate: this.readOnlyTemplate()
      },
      {
        prop: 'currentCounter',
        name: gT('bonuses.management.CurrentCounter'),
        width: 160,
        cellTemplate: this.readOnlyTemplate(),
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false
      },
      {
        prop: 'setting',
        name: gT('bonuses.management.Setting'),
        width: 160,
        cellTemplate: this.settingTemplate(),
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false
      }
    ];
  }

  ngOnDestroy() {
    this.saveToDisk();
  }

  refreshDataIndexes(data: Bonuses[]) {
    let index = 0;

    for (const i of data) {
      i.$$index = index++;
    }
  }

  onSearchChanged(value: string) {
    this.searchString = value;
    this.rows = this.rowsCache.filter(r =>
      Utilities.searchArray(value, false, r.phoneNumber));
  }

  showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  addClient() {
    this.formResetToggle = false;

    setTimeout(() => {
      this.formResetToggle = true;

      this.bonusEdit = {};
      this.modalService.open(this.editorModalTemplate());
    });
  }

  save() {
    this.bonusesService.saveBonuses(this.bonusEdit as Bonuses)
      .subscribe(() => {
        this.getBonuses(this.offset, this.limit, this.searchString);
      })

    return true;
  }

  updateSettingsValue(event: Event, row: Bonuses) {
    this.editing[row.$$index + '-setting'] = false;
    row.setting = parseInt((event.target as HTMLInputElement).value);
    this.rows = [...this.rows];

    this.saveToDisk();
  }

  updateNameValue(event: Event, row: Bonuses) {
    this.editing[row.$$index + '-name'] = false;
    row.name = (event.target as HTMLInputElement).value;
    this.rows = [...this.rows];

    this.saveToDisk();
  }

  delete(row: Bonuses) {
    this.alertService.showDialog('Are you sure you want to delete the client?', DialogType.confirm, () => this.deleteHelper(row));
  }

  deleteHelper(row: Bonuses) {
    this.rowsCache = this.rowsCache.filter(item => item !== row);
    this.rows = this.rows.filter(item => item !== row);

    this.saveToDisk();
  }

  getFromDisk() {
    return this.localStorage.getDataObject<Todo[]>(`${BonusesComponent.DBKeyBonuses}:${this.currentUserId}`);
  }

  saveToDisk() {
    if (this.isDataLoaded) {
      this.localStorage.saveSyncedSessionData(this.rowsCache, `${BonusesComponent.DBKeyBonuses}:${this.currentUserId}`);
    }
  }

  private getBonuses(offset: number, limit: number, search: string) {
    this.bonusesService.getBonuses(this.offset, this.limit, search)
      .subscribe(data =>
      {
        this.refreshDataIndexes(data)
        this.rows = data;
        this.rowsCache = [...data];
        this.isDataLoaded = true;
        setTimeout(() => { this.loadingIndicator = false; }, 1500);
      });
  }
}
