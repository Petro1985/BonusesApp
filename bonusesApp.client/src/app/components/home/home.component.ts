import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CdkDropList, CdkDrag, CdkDragPlaceholder } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';
import { fadeInOut } from '../../services/animations';
import { NotificationsViewerComponent } from '../controls/notifications-viewer.component';
import { BonusesComponent } from '../controls/bonuses.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  animations: [fadeInOut],
  standalone: true,
  imports: [
    CdkDropList, RouterLink, CdkDrag, CdkDragPlaceholder, NotificationsViewerComponent,
    BonusesComponent, TranslateModule, FormsModule
  ]
})
export class HomeComponent {
  phone: string = '';
}
