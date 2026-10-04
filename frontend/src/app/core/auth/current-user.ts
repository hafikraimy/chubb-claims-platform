import { UserRole } from './user-role';

export interface CurrentUser {
  id: string;
  name: string;
  role: UserRole;
  market: string;
  teamId: string | null;
}
