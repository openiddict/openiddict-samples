import { AuthTokenModel } from './auth-tokens-model';
import { ProfileModel } from './profile-model';

export interface AuthStateModel {
  tokens?: AuthTokenModel;
  profile?: ProfileModel;
  authReady?: boolean;
}
