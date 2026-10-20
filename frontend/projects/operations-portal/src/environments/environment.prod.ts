import { AuthEnv } from 'shared-ui';

export const environment: { auth: AuthEnv } = {
  auth: {
    auth: 'entra',
    clientId: '00000000-0000-0000-0000-000000000000',
    tenantId: '00000000-0000-0000-0000-000000000000',
    apiScope: 'api://00000000-0000-0000-0000-000000000000/access_as_user',
  },
};
