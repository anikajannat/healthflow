import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Doctor } from '../../core/models';

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './doctors.component.html',
  styleUrl: './doctors.component.css'
})
export class DoctorsComponent implements OnInit {
  doctors: Doctor[] = [];
  loading = true;
  error = '';
  selectedDoctorId = '';
  specialization = '';
  keyword = '';

  constructor(private api: ApiService) {}

  ngOnInit() { this.loadDoctors(); }

  loadDoctors() {
    this.loading = true;
    this.error = '';
    this.api.get<Doctor[]>('/doctors').subscribe({
      next: x => { this.doctors = x; this.loading = false; },
      error: e => { this.error = e.error?.message || 'Could not load doctors.'; this.loading = false; }
    });
  }

  get specializations() {
    return [...new Set(this.doctors.map(d => d.specialization).filter(Boolean))].sort();
  }

  get filteredDoctors() {
    const q = this.keyword.trim().toLowerCase();
    return this.doctors.filter(d => {
      const doctorMatch = !this.selectedDoctorId || d.id === this.selectedDoctorId;
      const specialtyMatch = !this.specialization || d.specialization === this.specialization;
      const keywordMatch = !q || [d.name, d.specialization, d.qualifications || '', d.bio || '']
        .some(v => v.toLowerCase().includes(q));
      return doctorMatch && specialtyMatch && keywordMatch;
    });
  }

  reset() {
    this.selectedDoctorId = '';
    this.specialization = '';
    this.keyword = '';
  }

  initials(name: string) {
    return name.split(/\s+/).filter(Boolean).slice(0, 2).map(x => x[0]).join('').toUpperCase();
  }
}
