import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Doctor, Schedule } from '../../core/models';

@Component({
  standalone: true,
  imports: [RouterLink],
  templateUrl: './doctor-detail.component.html',
  styleUrl: './doctor-detail.component.css'
})
export class DoctorDetailComponent implements OnInit {
  doctor?: Doctor;
  schedules: Schedule[] = [];
  loading = true;
  error = '';

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.get<Doctor>(`/doctors/${id}`).subscribe({
      next: d => { this.doctor = d; this.loading = false; },
      error: () => { this.error = 'Doctor profile could not be loaded.'; this.loading = false; }
    });
    this.api.get<Schedule[]>(`/schedules/doctor/${id}`, { days: '30' }).subscribe({ next: s => this.schedules = s });
  }

  initials(name: string) {
    return name.split(/\s+/).filter(Boolean).slice(0, 2).map(x => x[0]).join('').toUpperCase();
  }

  dateLabel(value: string) {
    return new Date(`${value}T00:00:00`).toLocaleDateString([], { weekday: 'short', day: 'numeric', month: 'short' });
  }
}
