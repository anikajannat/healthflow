import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  auth = inject(AuthService);
  year = new Date().getFullYear();
  dashboard = computed(() => {
    const role = this.auth.role();
    return role === 'Patient' ? '/patient' : role === 'Doctor' ? '/doctor' : role ? '/admin' : '/';
  });
}
