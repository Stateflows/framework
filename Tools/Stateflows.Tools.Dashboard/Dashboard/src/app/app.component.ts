import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'sf-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell">
      <aside class="sidebar">
        <div class="sidebar__logo">
          <img src="assets/stateflows-logo-blue.svg" alt="Stateflows" class="logo-img" />
        </div>
        <nav class="sidebar__nav">
          <a routerLink="overview" routerLinkActive="active" [routerLinkActiveOptions]="{exact:false}" class="nav-item">
            <i class="pi pi-home nav-icon"></i>
            <span>Overview</span>
          </a>
        </nav>
        <div class="sidebar__footer">Dashboard v1</div>
      </aside>
      <main class="main">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .shell { display: flex; height: 100vh; overflow: hidden; }

    .sidebar {
      width: var(--sf-sidebar-width);
      background: var(--sf-sidebar-bg);
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
    }

    .sidebar__logo {
      padding: 20px 16px 16px;
      border-bottom: 1px solid rgba(255,255,255,0.08);
    }
    .logo-img { width: 100%; max-width: 160px; display: block; }

    .sidebar__nav {
      flex: 1;
      padding: 12px 8px;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 12px;
      border-radius: 7px;
      color: rgba(255,255,255,0.65);
      text-decoration: none;
      font-weight: 500;
      font-size: 13px;
      transition: background 0.15s, color 0.15s;
      border-left: 3px solid transparent;
    }
    .nav-item:hover {
      background: rgba(255,255,255,0.07);
      color: #fff;
    }
    .nav-item.active {
      background: rgba(0,137,191,0.18);
      color: #fff;
      border-left-color: var(--sf-brand);
    }
    .nav-icon { font-size: 15px; min-width: 18px; }

    .sidebar__footer {
      padding: 12px 16px;
      font-size: 11px;
      color: rgba(255,255,255,0.3);
      border-top: 1px solid rgba(255,255,255,0.08);
    }

    .main { flex: 1; overflow-y: auto; padding: 28px; background: var(--sf-bg); }
  `],
})
export class AppComponent {}