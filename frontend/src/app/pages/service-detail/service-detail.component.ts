import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

type ServiceInfo = {
  title: string;
  eyebrow: string;
  summary: string;
  description: string;
  includes: string[];
  steps: { title: string; text: string }[];
  suitableFor: string[];
  cta: string;
};

const SERVICES: Record<string, ServiceInfo> = {
  'family-physician': {
    title: 'Family Physician and First Aid Care',
    eyebrow: 'Primary care',
    summary: 'Start with a trusted first point of care for common health concerns, routine guidance and minor first-aid needs.',
    description: 'HealthFlow helps patients connect with available primary-care doctors for initial consultation, follow-up and guidance on the next appropriate step when specialist care may be needed.',
    includes: [
      'General physician consultation',
      'Common symptom and health-concern review',
      'Basic first-aid and minor-injury guidance',
      'Routine follow-up consultation',
      'Referral guidance when specialist care is appropriate'
    ],
    steps: [
      { title: 'Choose a doctor', text: 'Browse approved doctors and review their specialty, fee and profile.' },
      { title: 'Select a schedule', text: 'Choose a published date and available appointment slot.' },
      { title: 'Book your consultation', text: 'Confirm the appointment and keep the booking details in your dashboard.' }
    ],
    suitableFor: ['General health concerns', 'Routine follow-up', 'Minor first-aid guidance', 'Patients unsure which specialist to choose'],
    cta: 'Find a family physician'
  },
  'specialist-consultations': {
    title: 'Specialist Doctor Consultations',
    eyebrow: 'Specialist care',
    summary: 'Find an approved specialist and book an appointment based on specialty and published availability.',
    description: 'Use HealthFlow to search doctors by name or specialty, review professional information and book a consultation from the doctor’s available schedule.',
    includes: [
      'Specialty-based doctor search',
      'Doctor profile and qualification information',
      'Published schedule and appointment-slot selection',
      'Consultation fee visibility',
      'Follow-up appointment support'
    ],
    steps: [
      { title: 'Search by specialty', text: 'Use the Doctors page to find a clinician in the specialty you need.' },
      { title: 'Review availability', text: 'Open the doctor’s schedule and choose a suitable available slot.' },
      { title: 'Manage the visit', text: 'Track the appointment from your patient dashboard.' }
    ],
    suitableFor: ['Specialist consultation', 'Second consultation', 'Ongoing specialist follow-up', 'Patients with an existing referral'],
    cta: 'Browse specialists'
  },
  'health-wellness': {
    title: 'Health and Wellness',
    eyebrow: 'Preventive care',
    summary: 'Support everyday wellbeing with preventive-health conversations, lifestyle guidance and routine follow-up.',
    description: 'This service area is designed for non-emergency wellness and preventive-care appointments. Patients can use the platform to connect with suitable clinicians and keep follow-up visits organised.',
    includes: [
      'Preventive-health consultation',
      'Lifestyle and wellbeing discussion',
      'Routine health follow-up',
      'General screening guidance',
      'Personal health-goal discussion'
    ],
    steps: [
      { title: 'Identify your goal', text: 'Decide whether you need a routine check, wellness discussion or follow-up.' },
      { title: 'Choose a clinician', text: 'Find an approved doctor whose profile matches your need.' },
      { title: 'Book and follow up', text: 'Reserve an available slot and keep future follow-ups in one place.' }
    ],
    suitableFor: ['Routine wellness visits', 'Preventive-health discussions', 'Lifestyle guidance', 'General health follow-up'],
    cta: 'Explore available doctors'
  },
  'dental-care': {
    title: 'Dental Care',
    eyebrow: 'Oral health',
    summary: 'Connect with dental-care professionals for consultation, assessment and planned follow-up.',
    description: 'HealthFlow can present dental practitioners alongside their profiles and schedules so patients can arrange a consultation without calling multiple clinics.',
    includes: [
      'Dental consultation booking',
      'Oral-health assessment appointments',
      'Follow-up scheduling',
      'Dentist profile information',
      'Appointment history in the patient dashboard'
    ],
    steps: [
      { title: 'Find dental care', text: 'Search for a dentist or dental specialty from the doctor directory.' },
      { title: 'Pick an appointment', text: 'Choose from the practitioner’s published schedule.' },
      { title: 'Keep your follow-up organised', text: 'Use the dashboard to review appointment information.' }
    ],
    suitableFor: ['Routine dental consultation', 'Oral-health concerns', 'Planned dental follow-up', 'Dental assessment'],
    cta: 'Find dental care'
  },
  'diagnostics': {
    title: 'Diagnostics',
    eyebrow: 'Diagnostic support',
    summary: 'Keep diagnostic-related consultations and follow-up organised through one healthcare journey.',
    description: 'The Diagnostics service page provides a clear starting point for patients who need clinical guidance around diagnostic tests, result review or follow-up. Test availability itself depends on the connected healthcare provider.',
    includes: [
      'Pre-test consultation guidance',
      'Diagnostic-result follow-up',
      'Relevant specialist appointment booking',
      'Clear next-step guidance from a clinician',
      'Centralised appointment history'
    ],
    steps: [
      { title: 'Start with a consultation', text: 'Book a clinician to discuss what kind of diagnostic follow-up you need.' },
      { title: 'Complete required tests', text: 'Use the provider or facility recommended for your care plan.' },
      { title: 'Book result review', text: 'Arrange a follow-up appointment to discuss results with a clinician.' }
    ],
    suitableFor: ['Diagnostic follow-up', 'Result review', 'Patients referred for testing', 'People seeking the right specialist after a test'],
    cta: 'Book a consultation'
  },
  'pharmacy': {
    title: 'Pharmacy',
    eyebrow: 'Medication support',
    summary: 'A service area for prescription-related support and medication follow-up within the HealthFlow care journey.',
    description: 'HealthFlow keeps prescription-related information connected to the patient’s consultation flow. Pharmacy availability, fulfilment and delivery can be integrated with a pharmacy provider when that service is enabled.',
    includes: [
      'Prescription-linked care information',
      'Medication follow-up appointments',
      'Prescription history support',
      'Clinician follow-up for medication questions',
      'Future pharmacy-provider integration point'
    ],
    steps: [
      { title: 'Consult a clinician', text: 'Use an appointment for assessment and prescription when clinically appropriate.' },
      { title: 'Review the prescription', text: 'Keep prescription information associated with your healthcare record.' },
      { title: 'Arrange follow-up', text: 'Book another consultation when your clinician recommends review.' }
    ],
    suitableFor: ['Prescription follow-up', 'Medication review appointments', 'Patients managing repeat consultations', 'Future pharmacy fulfilment integration'],
    cta: 'Find a doctor'
  }
};

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './service-detail.component.html',
  styleUrl: './service-detail.component.css'
})
export class ServiceDetailComponent implements OnInit, OnDestroy {
  service!: ServiceInfo;
  private sub?: Subscription;

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(params => {
      const slug = params.get('slug') ?? '';
      const selected = SERVICES[slug];
      if (!selected) {
        this.router.navigateByUrl('/');
        return;
      }
      this.service = selected;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
