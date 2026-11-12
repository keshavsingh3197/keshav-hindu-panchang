import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  if (auth.isAuthenticated()) {
    return true;
  }
  auth.loginRedirect();
  return false;
};

export const editorGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  if (auth.isAuthenticated() && auth.isEditorOrAdmin()) {
    return true;
  }
  if (!auth.isAuthenticated()) {
    auth.loginRedirect();
  } else {
    alert('Access restricted: Editor or Admin role required.');
  }
  return false;
};
