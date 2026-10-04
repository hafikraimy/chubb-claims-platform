import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DEMO_USERS } from '../../../core/auth/demo-users';
import { SessionService } from '../../../core/auth/session.service';

@Component({
  selector: 'app-user-selection-page',
  imports: [MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './user-selection-page.html',
  styleUrl: './user-selection-page.scss',
})
export class UserSelectionPage {
  private readonly session = inject(SessionService);

  protected readonly users = DEMO_USERS;
  protected readonly currentUser = this.session.currentUser;
  protected readonly selectingUserId = signal<string | null>(null);
  protected readonly isSelecting = computed(() => this.selectingUserId() !== null);
  protected readonly errorMessage = signal<string | null>(null);

  protected selectUser(userId: string): void {
    this.selectingUserId.set(userId);
    this.errorMessage.set(null);

    this.session.selectUser(userId).subscribe({
      next: () => this.selectingUserId.set(null),
      error: () => {
        this.selectingUserId.set(null);
        this.errorMessage.set('Unable to start the demo session. Check that the API is running.');
      },
    });
  }
}
