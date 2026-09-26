import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/api.service';

@Component({standalone:true,imports:[FormsModule],templateUrl:'./profile.component.html'})
export class ProfileComponent implements OnInit{
  p:any={};message='';error='';
  constructor(public auth:AuthService,private api:ApiService){}
  ngOnInit(){this.auth.loadMe().subscribe(u=>this.p={...(u.patientProfile||{})});}
  save(){this.api.put<any>('/profile/patient',this.p).subscribe({next:r=>this.message=r.message,error:e=>this.error=e.error?.message||'Update failed.'})}
}
