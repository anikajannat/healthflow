import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({ standalone: true, imports: [ReactiveFormsModule], templateUrl: './doctor-apply.component.html' })
export class DoctorApplyComponent {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  sent=false; message=''; error=''; loading=false;
  form=this.fb.nonNullable.group({
    name:['',Validators.required],email:['',[Validators.required,Validators.email]],password:['',[Validators.required,Validators.minLength(8)]],otp:[''],
    licenseNumber:['',Validators.required],specialization:['',Validators.required],qualifications:[''],experienceYears:[0,[Validators.required,Validators.min(0)]],
    consultationFee:[800,[Validators.required,Validators.min(0)]],bio:['']
  });

  sendOtp(){
    const email=this.form.value.email;
    if(!email)return;
    this.api.post('/auth/send-otp',{email,purpose:'DoctorApplication'}).subscribe({
      next:()=>{this.sent=true;this.message='Verification code sent.'},
      error:e=>this.error=e.error?.message||'Could not send code.'
    });
  }

  submit(){
    if(this.form.invalid||!this.sent)return;
    this.loading=true;
    this.api.post('/auth/doctor-apply',this.form.getRawValue()).subscribe({
      next:()=>{this.message='Application submitted. An admin must approve it before you can log in.';this.loading=false;},
      error:e=>{this.error=e.error?.message||'Application failed.';this.loading=false;}
    });
  }
}
