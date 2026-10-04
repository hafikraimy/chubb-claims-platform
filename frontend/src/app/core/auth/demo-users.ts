import { UserRole } from './user-role';

export interface DemoUserOption {
  id: string;
  name: string;
  role: UserRole;
  roleLabel: string;
  initials: string;
  description: string;
}

export const DEMO_USERS: readonly DemoUserOption[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    name: 'Hafiz Claimant',
    role: UserRole.Claimant,
    roleLabel: 'Claimant',
    initials: 'HC',
    description: 'Report incidents and track personal claims.',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    name: 'Ben Claims Officer',
    role: UserRole.ClaimsOfficer,
    roleLabel: 'Claims officer',
    initials: 'BO',
    description: 'Pick up, assess, and decide claims.',
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    name: 'Chen Claims Officer',
    role: UserRole.ClaimsOfficer,
    roleLabel: 'Claims officer',
    initials: 'CO',
    description: 'Review an alternative officer workload.',
  },
  {
    id: '99999999-9999-9999-9999-999999999999',
    name: 'Aisha Manager',
    role: UserRole.Manager,
    roleLabel: 'Manager',
    initials: 'AM',
    description: 'Monitor team workload and assign claims.',
  },
];
