import { DOCUMENT, inject, Service } from '@angular/core';

const DEMO_USER_STORAGE_KEY = 'claims-platform.demo-user-id';

@Service()
export class DemoIdentityStore {
  private readonly storage = inject(DOCUMENT).defaultView?.localStorage;

  read(): string | null {
    return this.storage?.getItem(DEMO_USER_STORAGE_KEY) ?? null;
  }

  write(userId: string): void {
    this.storage?.setItem(DEMO_USER_STORAGE_KEY, userId);
  }

  clear(): void {
    this.storage?.removeItem(DEMO_USER_STORAGE_KEY);
  }
}
