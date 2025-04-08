import { Component, OnInit, TemplateRef, inject, input, viewChild } from '@angular/core';
import {CommonModule, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgbModal, NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { TableColumn, NgxDatatableModule } from '@siemens/ngx-datatable';

import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
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
export class BonusesComponent implements OnInit {
  private alertService = inject(AlertService);
  private translationService = inject(AppTranslationService);
  private authService = inject(AuthService);
  private modalService = inject(NgbModal);
  private bonusesService = inject(BonusesEndpoint);

  gT = (key: string) => this.translationService.getTranslation(key);

  columns: TableColumn[] = [];
  data: Bonuses[] = [];
  rowsCache: Bonuses[] = [];
  editing: Record<string, boolean> = {};
  bonusEdit: Partial<Bonuses> = {};
  isDataLoaded = false;
  loadingIndicator = true;
  formResetToggle = true;
  private _currentUserId: string | undefined;
  private searchString: string = '';

  offset: number = 0;
  pageSize: number = 10;
  totalItems: number = 0;

  // защита от слишком частого обновления записи
  private debounceTimeoutDict: Map<number, ReturnType<typeof setTimeout>> = new Map<number, ReturnType<typeof setTimeout>>();
  private TimeOut: number = 1000;

  get currentUserId() {
    if (this.authService.currentUser) {
      this._currentUserId = this.authService.currentUser.id;
    }

    return this._currentUserId;
  }

  readonly verticalScrollbar = input(false);

  readonly currentCountTemplate = viewChild.required<TemplateRef<unknown>>('currentCountTemplate');

  readonly nameTemplate = viewChild.required<TemplateRef<unknown>>('nameTemplate');

  readonly settingTemplate = viewChild.required<TemplateRef<unknown>>('settingTemplate');

  readonly readOnlyTemplate = viewChild.required<TemplateRef<unknown>>('readOnlyTemplate');

  readonly deleteTemplate = viewChild.required<TemplateRef<unknown>>('deleteTemplate');

  readonly giveBonusTemplate = viewChild.required<TemplateRef<unknown>>('giveBonusTemplate');

  readonly editorModalTemplate = viewChild.required<TemplateRef<unknown>>('editorModal');

  ngOnInit() {
    this.loadingIndicator = true;

    this.getBonuses()

    this.columns = [
      {
        prop: '',
        name: '',
        width: 30,
        cellTemplate: this.deleteTemplate(),
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false
      },
      {
        prop: 'phoneNumber',
        name: this.gT('bonuses.management.PhoneNumber'),
        width: 180,
        resizeable: false,
        canAutoResize: false,
        sortable: false,
        draggable: false,
        cellTemplate: this.readOnlyTemplate()
      },
      {
        prop: 'name',
        name: this.gT('bonuses.management.Name'),
        width: 300,
        minWidth: 180,
        cellTemplate: this.nameTemplate(),
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false
      },
      {
        prop: 'totalCounter',
        name: this.gT('bonuses.management.TotalCounter'),
        width: 100,
        minWidth: 70,
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false,
        cellTemplate: this.readOnlyTemplate()
      },
      {
        prop: 'currentCounter',
        name: this.gT('bonuses.management.CurrentCounter'),
        width: 140,
        minWidth: 60,
        cellTemplate: this.currentCountTemplate(),
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false
      },
      {
        prop: 'setting',
        name: this.gT('bonuses.management.Setting'),
        width: 160,
        minWidth: 70,
        cellTemplate: this.settingTemplate(),
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false
      },
      {
        prop: '',
        name: this.gT('bonuses.management.GiveBonus'),
        width: 150,
        minWidth: 70,
        cellTemplate: this.giveBonusTemplate(),
        resizeable: false,
        canAutoResize: true,
        sortable: false,
        draggable: false
      }
    ];
  }

  refreshDataIndexes(data: Bonuses[]) {
    let index = 0;

    for (const i of data) {
      i.$$index = index++;
    }
  }

  onSearchChanged(value: string) {
    this.searchString = value;
    this.getBonuses();
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
    this.bonusesService.saveNewBonuses(this.bonusEdit as Bonuses)
      .subscribe(() => {
        this.getBonuses();
      })

    return true;
  }

  updateSettingsValue(event: any, row: Bonuses) {
    row.setting = event.target.value
    this.editing[row.$$index + '-setting'] = false;

    this.updateBonusEntry(row);
  }

  updateNameValue(event: any, row: Bonuses) {
    row.name = event.target.value
    this.editing[row.$$index + '-name'] = false;

    this.updateBonusEntry(row);
  }

  delete(row: Bonuses) {
    const formattedMessage = this.gT('bonuses.management.DeleteQuestion')
    const message = formattedMessage.replace("{0}", row.name).replace("{1}", row.phoneNumber);
    this.alertService.showDialog(message, DialogType.confirm, () => this.deleteHelper(row));
  }

  deleteHelper(row: Bonuses) {
    this.bonusesService.deleteBonuses(row)
      .subscribe(() =>
      {
        this.getBonuses();
      });
  }

  private getBonuses(withClearing: boolean = false) {
    if (withClearing)
    {
      this.totalItems = 0;
      this.data = [];
    }

    this.bonusesService.getBonuses(this.offset, this.pageSize, this.searchString)
      .subscribe(data =>
      {
        if (this.offset != 0 && data.bonuses.length === 0)
        {
          this.offset = 0;
          this.getBonuses()
        }
        else
        {
          this.totalItems = data.totalCount;
          this.refreshDataIndexes(data.bonuses)
          this.data = data.bonuses;
          this.rowsCache = [...data.bonuses];
          this.isDataLoaded = true;
          setTimeout(() => { this.loadingIndicator = false; }, 1500);
        }
      });
  }

  onPage($event: {offset: number, pageSize: number}) {
    this.offset = $event.offset;
    this.totalItems = $event.offset;
    this.pageSize = $event.pageSize;
    this.getBonuses(true);
  }

  addCount(row: Bonuses) {
    row.currentCounter++;
    row.totalCounter++;

    this.updateBonusEntry(row);
  }

  private updateBonusEntry(row: Bonuses) {
    if (this.debounceTimeoutDict.has(row.id)) {
      clearTimeout(this.debounceTimeoutDict.get(row.id));
    }
    this.debounceTimeoutDict.set(row.id, setTimeout(() => {
      this.bonusesService.updateBonuses(row)
        .subscribe(() =>
        {
        });
      this.debounceTimeoutDict.delete(row.id);
    }, this.TimeOut));
  }

  giveBonus(row: Bonuses) {
    const formattedMessage = this.gT('bonuses.management.GiveBonusQuestion');
    const message = formattedMessage.replace("{0}", row.name).replace("{1}", row.phoneNumber);

    this.alertService.showDialog(message, DialogType.confirm, () => this.giveBonusHelper(row));
  }

  private giveBonusHelper(row: Bonuses) {
    row.currentCounter -= row.setting;
    this.bonusesService.giveBonus(row.id)
      .subscribe(() =>
      {
        this.getBonuses();
      });
  }

  downloadBonuses() {
    this.bonusesService.getBonuses(0, 2_147_483_647, '')
      .subscribe(x =>
      {
        const data = x.bonuses;
        const csvRows = [];

        const firstLine = this.columns.slice(0, -1).map(x => x.name).join(';');
        // Заголовки
        const headers = Object.keys(data[0]);
        csvRows.push(firstLine);

        // Данные
        for (const row of data) {
          const values = headers.map(header => {
            const escaped = ('' + row[header as keyof Bonuses]).replace(/"/g, '\\"');
            return `"${escaped}"`;
          });
          csvRows.push(values.join(';'));
        }

        const csvContent = '\uFEFF' + csvRows.join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.setAttribute('href', url);
        a.setAttribute('download', 'data.csv');
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
      });
  }

  protected readonly Math = Math;
}
