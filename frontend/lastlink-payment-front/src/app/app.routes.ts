import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { AnticipationListComponent } from './features/anticipation/list/anticipation-list.component';
import { AnticipationCreateComponent } from './features/anticipation/create/anticipation-create.component';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'anticipations', component: AnticipationListComponent },
      { path: 'anticipations/new', component: AnticipationCreateComponent },
      { path: '', redirectTo: 'anticipations', pathMatch: 'full' }
    ]
  }
];
