import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BookingResponse, Doctor, Schedule } from '../../core/models';

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.css'
})
export class BookAppointmentComponent implements OnInit {
  doctor?: Doctor;
  schedules: Schedule[] = [];
  selectedDate = '';
  selectedSchedule?: Schedule;
  selectedSlot: number | null = null;
  reason = '';
  bkashNumber = '';
  agree = false;
  step = 1;
  loading = true;
  paying = false;
  error = '';
  success?: BookingResponse;

  constructor(private route: ActivatedRoute, private api: ApiService, public auth: AuthService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('doctorId')!;
    this.api.get<Doctor>(`/doctors/${id}`).subscribe({ next: d => this.doctor = d });
    this.api.get<Schedule[]>(`/schedules/doctor/${id}`, { days: '30' }).subscribe({
      next: schedules => {
        this.schedules = schedules;
        this.selectedDate = this.dates[0] || '';
        this.pickFirstSchedule();
        this.loading = false;
      },
      error: e => { this.error = e.error?.message || 'Could not load doctor schedules.'; this.loading = false; }
    });
  }

  get dates() { return [...new Set(this.schedules.map(s => s.date))]; }
  get schedulesForDate() { return this.schedules.filter(s => s.date === this.selectedDate); }

  selectDate(date: string) {
    this.selectedDate = date;
    this.selectedSlot = null;
    this.pickFirstSchedule();
  }

  pickFirstSchedule() { this.selectedSchedule = this.schedulesForDate[0]; }

  dateLabel(value: string) {
    return new Date(`${value}T00:00:00`).toLocaleDateString([], { weekday: 'short', day: 'numeric', month: 'short' });
  }

  slots(s: Schedule) {
    const [h, m] = s.startTime.split(':').map(Number);
    const blocked = new Set(s.unavailableSlots || s.bookedSlots || []);
    return Array.from({ length: s.totalSlots }, (_, i) => ({
      index: i,
      label: new Date(2000, 0, 1, h, m + i * 20).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      unavailable: blocked.has(i)
    }));
  }

  chooseSlot(index: number) { this.selectedSlot = index; this.error = ''; }

  selectedSlotLabel() {
    if (!this.selectedSchedule || this.selectedSlot === null) return '';
    return this.slots(this.selectedSchedule).find(x => x.index === this.selectedSlot)?.label || '';
  }

  next() {
    this.error = '';
    if (this.step === 1 && (!this.selectedSchedule || this.selectedSlot === null)) {
      this.error = 'Choose an available date and time slot first.'; return;
    }
    if (this.step < 3) this.step++;
  }

  back() { if (this.step > 1) this.step--; this.error = ''; }

  payAndBook() {
    if (!this.auth.isLoggedIn() || this.auth.role() !== 'Patient') {
      this.error = 'Please log in with a patient account before booking.'; return;
    }
    if (!this.selectedSchedule || this.selectedSlot === null) return;
    if (!/^01\d{9}$/.test(this.bkashNumber.replace(/\D/g, ''))) {
      this.error = 'Enter a valid 11-digit bKash mobile number.'; return;
    }
    if (!this.agree) { this.error = 'Please confirm the booking and payment terms.'; return; }

    this.paying = true;
    this.error = '';
    this.api.post<BookingResponse>('/appointments/book', {
      scheduleId: this.selectedSchedule.id,
      slotIndex: this.selectedSlot,
      reason: this.reason,
      paymentMethod: 'Bkash',
      bkashNumber: this.bkashNumber
    }).subscribe({
      next: r => { this.success = r; this.paying = false; this.step = 4; },
      error: e => { this.error = e.error?.message || 'Booking or payment failed.'; this.paying = false; }
    });
  }
}
