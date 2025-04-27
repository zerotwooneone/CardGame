import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    // Lazy load the standalone LoginComponent
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
    // Add guards here later if needed (e.g., prevent access if already logged in)
  },
  {
    path: 'lobby',
    // Example route for after login
    loadComponent: () => import('./features/lobby/lobby.component').then(m => m.LobbyComponent), // Assuming LobbyComponent exists
    // canActivate: [AuthGuard] // Add AuthGuard later
  },
  {
    path: 'game/:id',
    // Example route for game view
    loadComponent: () => import('./features/game/game-view/game-view.component').then(m => m.GameViewComponent), // Assuming GameViewComponent exists
    // canActivate: [AuthGuard] // Add AuthGuard later
  },
  // Default route
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  // Wildcard route for 404
  { path: '**', redirectTo: '/login' } // Or a dedicated NotFoundComponent
];
