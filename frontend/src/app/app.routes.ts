import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'apply-doctor', loadComponent: () => import('./pages/doctor-apply/doctor-apply.component').then(m => m.DoctorApplyComponent) },
  { path: 'doctors', loadComponent: () => import('./pages/doctors/doctors.component').then(m => m.DoctorsComponent) },
  { path: 'doctors/:id', loadComponent: () => import('./pages/doctor-detail/doctor-detail.component').then(m => m.DoctorDetailComponent) },
  { path: 'book/:doctorId', loadComponent: () => import('./pages/book-appointment/book-appointment.component').then(m => m.BookAppointmentComponent) },
  { path: 'services/:slug', loadComponent: () => import('./pages/service-detail/service-detail.component').then(m => m.ServiceDetailComponent) },
  { path: 'patient', canActivate: [authGuard, roleGuard(['Patient'])], loadComponent: () => import('./pages/patient-dashboard/patient-dashboard.component').then(m => m.PatientDashboardComponent) },
  { path: 'doctor', canActivate: [authGuard, roleGuard(['Doctor'])], loadComponent: () => import('./pages/doctor-dashboard/doctor-dashboard.component').then(m => m.DoctorDashboardComponent) },
  { path: 'admin', canActivate: [authGuard, roleGuard(['Admin', 'SuperAdmin'])], loadComponent: () => import('./pages/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent) },
  { path: '**', redirectTo: '' }
];
